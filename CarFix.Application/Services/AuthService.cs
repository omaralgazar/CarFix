using CarFix.Application.Configuration;
using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using CarFix.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;


namespace CarFix.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IServiceCenterRepository _serviceCenterRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly BootstrapSuperAdminSettings _bootstrapSuperAdminSettings;
        private readonly JwtSettings _jwtSettings;
        public AuthService(IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository ,IServiceCenterRepository serviceCenterRepository , IOptions<JwtSettings> jwtSettings , IOptions<BootstrapSuperAdminSettings> bootstrapSuperAdminSettings , IPasswordResetTokenRepository passwordResetTokenRepository , IEmailSender emailSender)
        {
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _serviceCenterRepository = serviceCenterRepository;
            _bootstrapSuperAdminSettings = bootstrapSuperAdminSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailSender = emailSender;
        }

        private static string ComputeSha256Hash(string input)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        private async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId)
        {
            var refreshTokenPlainText = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = ComputeSha256Hash(refreshTokenPlainText),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return refreshTokenPlainText;
        }
        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ConflictException("This Email already exists.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRoles.Customer,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            var refreshToken = await GenerateAndStoreRefreshTokenAsync(newUser.Id);

            return new AuthResponseDto
            {
                AccessToken = _tokenGenerator.GenerateToken(newUser),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes)
            };
        }

        public async Task<AuthResponseDto> RegisterCenterAsync(RegisterCenterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ConflictException("User with this email already exists.");
            }
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRoles.ServiceCenter,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(newUser);
            var newCenter = new ServiceCenter
            {
                Id = Guid.NewGuid(),
                OwnerUserId = newUser.Id,
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                Rating = 0,
                VerificationStatus = VerificationStatus.Pending,
                IsBanned = false,
                ConsecutiveDelayCount = 0,
                BanEscalationCount = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _serviceCenterRepository.AddAsync(newCenter);
            await _userRepository.SaveChangesAsync();

            var refreshToken = await GenerateAndStoreRefreshTokenAsync(newUser.Id);
            return new AuthResponseDto
            {
                AccessToken = _tokenGenerator.GenerateToken(newUser),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var (isValid, needsRehash) = _passwordHasher.VerifyPassword(user.PasswordHash, dto.Password);
            if (!isValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (needsRehash)
            {
                user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
                await _userRepository.SaveChangesAsync();
            }

            var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                AccessToken = _tokenGenerator.GenerateToken(user),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = ComputeSha256Hash(refreshToken);
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash);

            if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = await _userRepository.GetByIdAsync(storedRefreshToken.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            storedRefreshToken.IsRevoked = true;  

            var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);   

            return new AuthResponseDto
            {
                AccessToken = _tokenGenerator.GenerateToken(user),
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var tokenHash = ComputeSha256Hash(refreshToken);
        
            var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (token == null)
                return;

            await _refreshTokenRepository.RevokeAsync(token);

            await _refreshTokenRepository.SaveChangesAsync();
        }


        public async Task<AuthResponseDto> BootstrapSuperAdminAsync(RegisterDto dto,string bootstrapKey)
        {
            if (await _userRepository.HasSuperAdminAsync())
                throw new NotFoundException("Bootstrap setup is no longer available.");

            if (string.IsNullOrWhiteSpace(
                    _bootstrapSuperAdminSettings.key))
            {
                throw new InvalidOperationException(
                    "Bootstrap super admin key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(bootstrapKey) ||
                !KeysMatch(
                    bootstrapKey,
                    _bootstrapSuperAdminSettings.key))
            {
                throw new UnauthorizedAccessException(
                    "Invalid bootstrap key.");
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository
                .GetByEmailAsync(email);

            if (existingUser != null)
                throw new ConflictException(
                    "A user with this email already exists.");

            var superAdmin = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = email,
                Phone = dto.Phone.Trim(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRoles.SuperAdmin,
                IsEmailVerified = true,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(superAdmin);
            await _userRepository.SaveChangesAsync();

            var refreshToken = await GenerateAndStoreRefreshTokenAsync(
                superAdmin.Id);

            return new AuthResponseDto
            {
                AccessToken = _tokenGenerator.GenerateToken(superAdmin),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    _jwtSettings.AccessTokenMinutes)
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return;

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.IsDeleted)
                return;

            await _passwordResetTokenRepository
                .RevokeActiveTokensForUserAsync(user.Id);

            var resetToken = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            var passwordResetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = ComputeSha256Hash(resetToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _passwordResetTokenRepository.AddAsync(passwordResetToken);

            await _passwordResetTokenRepository.SaveChangesAsync();

            await _emailSender.SendPasswordResetEmailAsync(
                user.Email,
                resetToken);
        }


        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new BadRequestException("Reset token is required.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new BadRequestException("New password is required.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException(
                    "New password and confirmation password do not match.");

            var tokenHash = ComputeSha256Hash(dto.Token);

            var passwordResetToken = await _passwordResetTokenRepository
                .GetByTokenHashAsync(tokenHash);

            if (passwordResetToken == null ||
                passwordResetToken.UsedAt != null ||
                passwordResetToken.RevokedAt != null ||
                passwordResetToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Invalid or expired reset token.");
            }

            var user = await _userRepository
                .GetByIdAsync(passwordResetToken.UserId);

            if (user == null || user.IsDeleted)
                throw new BadRequestException(
                    "Invalid or expired reset token.");

            user.PasswordHash = _passwordHasher
                .HashPassword(dto.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            passwordResetToken.UsedAt = DateTime.UtcNow;

            await _passwordResetTokenRepository
                .RevokeActiveTokensForUserAsync(user.Id);

            await _refreshTokenRepository
                .RevokeAllByUserIdAsync(user.Id);

            await _passwordResetTokenRepository.SaveChangesAsync();
        }

        #region Helper
        private static bool KeysMatch(string providedKey,string configuredKey)
        {
            var providedBytes = System.Text.Encoding.UTF8
                .GetBytes(providedKey);

            var configuredBytes = System.Text.Encoding.UTF8
                .GetBytes(configuredKey);

            return System.Security.Cryptography.CryptographicOperations
                .FixedTimeEquals(providedBytes, configuredBytes);
        }
        #endregion

    }
}

