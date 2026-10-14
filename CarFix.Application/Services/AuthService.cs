using CarFix.Application.Configuration;
using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
using CarFix.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


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
        private readonly ILogger<AuthService> _logger;
        private readonly BootstrapSuperAdminSettings _bootstrapSuperAdminSettings;
        private readonly JwtSettings _jwtSettings;

        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan FailedLoginWindow = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan BaseLockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MaxLockoutDuration = TimeSpan.FromHours(24);
        private const string AccountLockedMessage = "Account is temporarily locked. Check your email for details.";

        public AuthService(
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IServiceCenterRepository serviceCenterRepository,
            IOptions<JwtSettings> jwtSettings,
            IOptions<BootstrapSuperAdminSettings> bootstrapSuperAdminSettings,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailSender emailSender,
            ILogger<AuthService> logger)
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
            _logger = logger;
        }

        private static string ComputeSha256Hash(string input)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
        private static readonly TimeSpan RefreshReuseGracePeriod = TimeSpan.FromSeconds(10);

        private static string NormalizeEmail(string? email) =>
                                         (email ?? string.Empty).Trim().ToLowerInvariant();

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
            var email = NormalizeEmail(dto.Email);
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                throw new ConflictException("This Email already exists.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = email,
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
            var email = NormalizeEmail(dto.Email);
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new ConflictException("User with this email already exists.");
            }
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = email,
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
                Email = email,
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

        private static string? _dummyPasswordHash;

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            var password = dto.Password ?? string.Empty;

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.IsDeleted)
            {
                _dummyPasswordHash ??= _passwordHasher.HashPassword("dummy-password-for-timing");
                _passwordHasher.VerifyPassword(_dummyPasswordHash, password);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var (isValid, needsRehash) = _passwordHasher.VerifyPassword(user.PasswordHash, password);

            if (IsLocked(user))
                throw new ForbiddenException(AccountLockedMessage);

            if (!isValid)
            {
                await RegisterFailedLoginAsync(user);

                // نفس الـ user object بعد الـ reload، فنعرف لو الطلب ده أو طلب تاني قفله
                if (IsLocked(user))
                    throw new ForbiddenException(AccountLockedMessage);

                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.Role == UserRoles.ServiceCenter)
            {
                var center = await _serviceCenterRepository.GetByOwnerIdAsync(user.Id);

                if (center == null || center.IsDeleted)
                    throw new UnauthorizedAccessException("Invalid email or password.");

                if (center.IsBanned)
                    throw new ForbiddenException("This service center account is banned.");
            }

            await SaveUserWithRetryAsync(user, u =>
            {
                if (needsRehash)
                    u.PasswordHash = _passwordHasher.HashPassword(password);

                u.FailedLoginAttempts = 0;
                u.LockoutCount = 0;
                u.LockoutEnd = null;
                u.LastFailedLoginAt = null;
            });

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

            if (storedRefreshToken == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            if (storedRefreshToken.IsRevoked)
            {
                var isReuse = storedRefreshToken.RotatedAt != null &&
                              DateTime.UtcNow - storedRefreshToken.RotatedAt.Value > RefreshReuseGracePeriod;

                if (isReuse)
                {
                    await _refreshTokenRepository.RevokeAllByUserIdAsync(storedRefreshToken.UserId);
                    await _refreshTokenRepository.SaveChangesAsync();
                }

                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            if (storedRefreshToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = await _userRepository.GetByIdAsync(storedRefreshToken.UserId);
            if (user == null || user.IsDeleted)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RotatedAt = DateTime.UtcNow;

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

            var email = NormalizeEmail(dto.Email);

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

            var email = NormalizeEmail(dto.Email);

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
            user.FailedLoginAttempts = 0;
            user.LockoutCount = 0;
            user.LockoutEnd = null;
            user.LastFailedLoginAt = null;

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


        private static bool IsLocked(User user) =>
    user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow;

        private static TimeSpan CalculateLockoutDuration(int lockoutCount)
        {
            var minutes = BaseLockoutDuration.TotalMinutes * Math.Pow(2, lockoutCount - 1);
            return TimeSpan.FromMinutes(Math.Min(minutes, MaxLockoutDuration.TotalMinutes));
        }

        private async Task SaveUserWithRetryAsync(User user, Action<User> apply)
        {
            for (var attempt = 1; ; attempt++)
            {
                apply(user);

                try
                {
                    await _userRepository.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateConcurrencyException) when (attempt < 3)
                {
                    await _userRepository.ReloadAsync(user);
                }
            }
        }

        private async Task RegisterFailedLoginAsync(User user)
        {
            TimeSpan? newLockDuration = null;

            await SaveUserWithRetryAsync(user, u =>
            {
                newLockDuration = null;

                // طلب تاني قفل الحساب قبل ما نوصل: ما نحسبش عليه محاولة جديدة
                if (IsLocked(u))
                    return;

                var now = DateTime.UtcNow;
                var windowExpired = u.LastFailedLoginAt == null ||
                                    now - u.LastFailedLoginAt.Value > FailedLoginWindow;

                u.FailedLoginAttempts = windowExpired ? 1 : u.FailedLoginAttempts + 1;
                u.LastFailedLoginAt = now;

                if (u.FailedLoginAttempts < MaxFailedLoginAttempts)
                    return;

                u.LockoutCount++;
                u.FailedLoginAttempts = 0;
                newLockDuration = CalculateLockoutDuration(u.LockoutCount);
                u.LockoutEnd = now.Add(newLockDuration.Value);
            });

            // الإيميل بيتبعت بس لما القفل يتطبق فعلاً في الطلب ده
            if (newLockDuration is { } duration)
            {
                try
                {
                    await _emailSender.SendAccountLockedEmailAsync(
                        user.Email,
                        (int)Math.Ceiling(duration.TotalMinutes));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send account-locked email for user {UserId}", user.Id);
                }
            }
        }
        #endregion

    }
}

