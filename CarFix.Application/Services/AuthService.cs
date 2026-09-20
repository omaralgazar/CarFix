using CarFix.Application.Configuration;
using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;
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
        private readonly JwtSettings _jwtSettings;
        public AuthService(IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository ,IServiceCenterRepository serviceCenterRepository , IOptions<JwtSettings> jwtSettings)
        {
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _serviceCenterRepository = serviceCenterRepository;
            _jwtSettings = jwtSettings.Value;
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
    }
}

