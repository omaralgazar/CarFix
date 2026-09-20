using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;


﻿using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterCustomerAsync(request);
            return Ok(result);
        }

        [HttpPost("register-center")]
        public async Task<ActionResult<AuthResponseDto>> RegisterCenter([FromBody] RegisterCenterDto request)
        {
            var result = await _authService.RegisterCenterAsync(request);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }
    }

}

