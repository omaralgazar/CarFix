using CarFix.Application.DTOs.ServiceCenter;
using CarFix.Application.Interfaces;
using CarFix.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCenterController : BaseController
    {
        private readonly IServiceCenterServices _serviceCenterService;

        public ServiceCenterController(IServiceCenterServices serviceCenterService)
        {
            _serviceCenterService = serviceCenterService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,ServiceCenter,Customer ,Staff")]
        public async Task<IActionResult> GetServiceCenterById(Guid id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Customer")
            {
                var publicProfile = await _serviceCenterService.GetPublicProfileAsync(id);
                return Ok(publicProfile);
            }

            var fullProfile = await _serviceCenterService.GetProfileAsync(id);
            return Ok(fullProfile);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetMyProfile()
        {
            var ownerUserId = GetCurrentUserId();
            var serviceCenter = await _serviceCenterService.GetProfileByOwnerIdAsync(ownerUserId);
            return Ok(serviceCenter);
        }


        [HttpPut("profile")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> UpdateServiceCenterProfile([FromBody] UpdateCenterProfileDto dto)
        {
            var ownerUserId = GetCurrentUserId();
            var serviceCenter = await _serviceCenterService.UpdateProfileAsync(ownerUserId, dto);
            return Ok(serviceCenter);
        }

        [HttpPost("specialties")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> AddServiceCenterSpecialty([FromBody] AddSpecialtyDto dto)
        {
            var ownerUserId = GetCurrentUserId();
            var serviceCenter = await _serviceCenterService.AddSpecialtyAsync(ownerUserId, dto);
            return Ok(serviceCenter);
        }

        [HttpPut("specialties/{specialtyId}")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> UpdateServiceCenterSpecialty(Guid specialtyId, [FromBody] UpdateCenterSpecialityDto dto)
        {
            var ownerUserId = GetCurrentUserId();
            var serviceCenter = await _serviceCenterService.UpdateSpecialtyAsync(ownerUserId, specialtyId, dto);
            return Ok(serviceCenter);
        }

        [HttpDelete("specialties/{specialtyId}")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> RemoveSpecialty(Guid specialtyId)
        {
            var ownerUserId = GetCurrentUserId();
            var serviceCenter = await _serviceCenterService.RemoveSpecialtyAsync(ownerUserId, specialtyId);
            return Ok(serviceCenter);
        }
    }
}