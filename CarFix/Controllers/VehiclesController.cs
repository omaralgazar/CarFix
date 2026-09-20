using CarFix.Application.DTOs.Customer.Vehicle;
using CarFix.Application.Services;
using Microsoft.AspNetCore.Authorization;


using CarFix.Domain.Entities;
using CarFix.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

namespace CarFix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    

    public class VehiclesController : BaseController
    {

       

        private readonly VehicleService _vehicleService;

        public VehiclesController(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(Guid id)
        {
            var customerId = GetCurrentUserId();
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id, customerId);
            return Ok(vehicle);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var customerId = GetCurrentUserId();
            var vehicles = await _vehicleService.ListVehiclesAsync(customerId);
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            var customerId = GetCurrentUserId();
            var vehicle = await _vehicleService.CreateVehicleAsync(customerId, dto);


            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, vehicle);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var customerId = GetCurrentUserId();
            await _vehicleService.DeleteVehicleAsync(id, customerId);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleDto dto)
        {
            var customerId = GetCurrentUserId();
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, customerId, dto);
            return Ok(vehicle);
        }

        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetDefaultVehicle(Guid id)
        {
            var customerId = GetCurrentUserId();
            await _vehicleService.SetDefaultVehicleAsync(id, customerId);
            return NoContent();
        }
    }

}



