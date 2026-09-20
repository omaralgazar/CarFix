using CarFix.Application.DTOs.AuthDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarFix.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto dto);
        Task<AuthResponseDto> RegisterCenterAsync(RegisterCenterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);


    }
}
