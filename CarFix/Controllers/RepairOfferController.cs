using CarFix.Application.DTOs.ServiceCenter.RepairOfferDto;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairOfferController : BaseController
    {
        private readonly IRepairOfferService _repairOfferService;
        public RepairOfferController(IRepairOfferService repairOfferService)
        {
            _repairOfferService = repairOfferService;
        }

        [HttpPost]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> CreateRepairOffer([FromBody] CreateOfferDto dto)
        {
            var serviceCenterId = GetCurrentUserId();
            var repairOffer = await _repairOfferService.CreateOfferAsync(serviceCenterId, dto);
            return Ok(repairOffer);
        }

        [HttpGet("request/{repairRequestId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOffersForRequestAsync( Guid repairRequestId)
        {
            var customerUserId = GetCurrentUserId();   
            var offers = await _repairOfferService.GetOffersForRequestAsync(customerUserId, repairRequestId);
            return Ok(offers);
        }

        [HttpGet("MyOffers")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetMyOffersAsync()
        {
            var centerUserId = GetCurrentUserId();
            var repairOffers = await _repairOfferService.GetMyOffersAsync(centerUserId);
            return Ok(repairOffers);
        }

        [HttpPatch("{offerId:guid}/withdraw")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> WithdrawOfferAsync(Guid offerId)
        {
            var serviceCenterId = GetCurrentUserId();
            await _repairOfferService.WithdrawOfferAsync(serviceCenterId, offerId);
            return Ok();
        }
    }
}
