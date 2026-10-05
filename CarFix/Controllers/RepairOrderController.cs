using CarFix.Application.DTOs.RepairOrder;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairOrderController : BaseController
    {
        private readonly IRepairOrderService _repairOrderService;

        public RepairOrderController(IRepairOrderService repairOrderService)
        {
            _repairOrderService = repairOrderService;
        }

        [HttpGet("{orderId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetRepairOrderForCustomerAsync(Guid orderId)
        {
            var currentUserId = GetCurrentUserId();

            var repairOrder = await _repairOrderService
                .GetTrackingForCustomerAsync(currentUserId, orderId);

            return Ok(repairOrder);
        }
        [HttpGet("{orderId:guid}/center")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetRepairOrderForServiceCenterAsync(Guid orderId)
        {
            var currentUserId = GetCurrentUserId();

            var repairOrder = await _repairOrderService
                .GetTrackingForServiceCenterAsync(currentUserId, orderId);

            return Ok(repairOrder);
        }

        [HttpPatch("requests/{repairRequestId:guid}/offers/{offerId:guid}/accept")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AcceptOfferAsync(Guid repairRequestId, Guid offerId)
        {
            var customerId = GetCurrentUserId();
            var orderTrackingResponse = await _repairOrderService
                .AcceptOfferAsync(customerId, repairRequestId, offerId);
            return CreatedAtAction(nameof(GetRepairOrderForCustomerAsync), new { orderId = orderTrackingResponse.OrderId }, orderTrackingResponse);
        }

        [HttpPost("{orderId:guid}/check-in-otp")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GenerateCheckInOtpAsync(Guid orderId)
        {
            var customerId = GetCurrentUserId();
            var otpResponse = await _repairOrderService
                .GenerateCheckInOtpAsync(customerId, orderId);
            return Ok(otpResponse);
        }

        [HttpPatch("{orderId:guid}/check-in")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> ConfirmCheckInAsync(Guid orderId, [FromBody] ConfirmCheckInDto dto)
        {
            var centerUserId = GetCurrentUserId();
            var orderTrackingResponse = await _repairOrderService
                .ConfirmCheckInAsync(centerUserId, orderId, dto);
            return Ok(orderTrackingResponse);
        }

        [HttpPost("{orderId:guid}/check-out-otp")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GenerateCheckOutOtpAsync(Guid orderId)
        {
            var customerId = GetCurrentUserId();
            var otpResponse = await _repairOrderService
                .GenerateCheckOutOtpAsync(customerId, orderId);
            return Ok(otpResponse);
        }

        [HttpPatch("{orderId:guid}/check-out")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> ConfirmCheckOutAsync(Guid orderId, [FromBody] ConfirmCheckOutDto dto)
        {
            var centerUserId = GetCurrentUserId();
            var orderTrackingResponse = await _repairOrderService
                .ConfirmCheckOutAsync(centerUserId, orderId, dto);
            return Ok(orderTrackingResponse);
        }

        [HttpPatch("{orderId:guid}/ready-for-handover")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> MarkReadyForHandoverAsync(Guid orderId)
        {
            var centerUserId = GetCurrentUserId();
            var orderTrackingResponse = await _repairOrderService
                .MarkReadyForHandoverAsync(centerUserId, orderId);
            return Ok(orderTrackingResponse);
        }
    }
}
