using CarFix.Application.DTOs.Customer.RepairRequest;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/repair-requests")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class RepairRequestController : BaseController
    {
        private readonly IRepairRequestService _repairRequestService;

        public RepairRequestController(
            IRepairRequestService repairRequestService)
        {
            _repairRequestService = repairRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRepairRequest(
            [FromBody] CreateRepairRequestDto dto)
        {
            var customerUserId = GetCurrentUserId();

            var repairRequest = await _repairRequestService
                .CreateRequestAsync(customerUserId, dto);

            return CreatedAtAction(
                nameof(GetRepairRequestById),
                new { requestId = repairRequest.Id },
                repairRequest);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRepairRequests()
        {
            var customerUserId = GetCurrentUserId();

            var repairRequests = await _repairRequestService
                .GetRepairRequestsByCustomerAsync(customerUserId);

            return Ok(repairRequests);
        }

        [HttpGet("{requestId:guid}")]
        public async Task<IActionResult> GetRepairRequestById(Guid requestId)
        {
            var customerUserId = GetCurrentUserId();

            var repairRequest = await _repairRequestService
                .GetRequestByIdAsync(customerUserId, requestId);

            return Ok(repairRequest);
        }

        [HttpPatch("{requestId:guid}/cancel")]
        public async Task<IActionResult> CancelRepairRequest(Guid requestId)
        {
            var customerUserId = GetCurrentUserId();

            await _repairRequestService
                .CancelRequestAsync(customerUserId, requestId);

            return NoContent();
        }
    }
}