using CarFix.Application.DTOs.Admin;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("service-centers/pending")]
        public async Task<IActionResult> GetAllPendingServiceCenters()
        {
            var pendingCenters = await _adminService
                .GetAllPendingServiceCentersAsync();

            return Ok(pendingCenters);
        }

        [HttpGet("service-centers/{centerId:guid}")]
        public async Task<IActionResult> GetServiceCenterDetails(Guid centerId)
        {
            var centerDetails = await _adminService
                .GetServiceCenterDetailsAsync(centerId);

            return Ok(centerDetails);
        }

        [HttpPatch("service-centers/{centerId:guid}/approve")]
        public async Task<IActionResult> ApproveServiceCenter(Guid centerId)
        {
            var adminId = GetCurrentUserId();

            await _adminService
                .ApproveServiceCenterAsync(adminId, centerId);

            return NoContent();
        }

        [HttpPatch("service-centers/{centerId:guid}/reject")]
        public async Task<IActionResult> RejectServiceCenter(
            Guid centerId,
            [FromBody] RejectServiceCenterDto dto)
        {
            var adminId = GetCurrentUserId();

            await _adminService
                .RejectServiceCenterAsync(adminId, centerId, dto);

            return NoContent();
        }
    }
}