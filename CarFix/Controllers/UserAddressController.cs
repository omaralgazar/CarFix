using CarFix.Application.DTOs.Customer.Address;
using CarFix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAddressController : BaseController
    {
        private readonly IUserAddressService _userAddressService;
        
        public UserAddressController(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateUserAddressDto addressDto)
        {
            var userId = GetCurrentUserId();
            var createdAddress = await _userAddressService.CreateAddressAsync(userId, addressDto);
            return CreatedAtAction(
                     nameof(GetAddressById),
                     new { addressId = createdAddress.Id },
                     createdAddress);
        }

        [HttpGet("MyAddresses")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            var addresses = await _userAddressService.GetAllAddressesByUserIdAsync(userId);
            return Ok(addresses);
        }

        [HttpGet("{addressId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAddressById(Guid addressId)
        {
            var userId = GetCurrentUserId();
            var address = await _userAddressService.GetAddressByIdAsync(addressId, userId);
            return Ok(address);
        }
        [HttpPut("{addressId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateUserAddressDto addressDto)
        {
            var userId = GetCurrentUserId();
            var updatedAddress = await _userAddressService.UpdateAddressAsync(addressId, userId, addressDto);
            return Ok(updatedAddress);
        }

        [HttpDelete("{addressId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            var userId = GetCurrentUserId();
            await _userAddressService.DeleteAddressAsync(addressId, userId);
            return NoContent();
        }

        [HttpPatch("{addressId:guid}/set-default")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SetDefaultAddress(Guid addressId)
        {
            var userId = GetCurrentUserId();
            await _userAddressService.SetDefaultAddressAsync(addressId, userId);
            return NoContent();
        }
    }
}
