using CarFix.Application.DTOs.ServiceCenter;
using CarFix.Application.Exceptions;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Domain.Entities;
using CarFix.Domain.Enums;

namespace CarFix.Application.Services
{
    public class ServiceCenterService : IServiceCenterServices
    {
        private readonly IServiceCenterRepository _repository;

        public ServiceCenterService(IServiceCenterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CenterProfileResponseDto> GetProfileAsync(
            Guid serviceCenterId)
        {
            var serviceCenter = await _repository.GetByIdAsync(serviceCenterId);

            if (serviceCenter == null || serviceCenter.IsDeleted)
                throw new NotFoundException("Service center not found.");

            return MapToProfileDto(serviceCenter);
        }

        public async Task<CenterProfileResponseDto> GetProfileByOwnerIdAsync(
            Guid ownerId)
        {
            var serviceCenter = await _repository.GetByOwnerIdAsync(ownerId);

            if (serviceCenter == null || serviceCenter.IsDeleted)
                throw new NotFoundException("Service center not found.");

            return MapToProfileDto(serviceCenter);
        }

        public async Task<CenterProfileResponseDto> UpdateProfileAsync(
            Guid ownerUserId,
            UpdateCenterProfileDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);

            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                center.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Address))
                center.Address = dto.Address.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                center.Phone = dto.Phone.Trim();

            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> AddCapabilityAsync(
            Guid ownerUserId,
            AddCenterCapabilityDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);

            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            var issueCategory = NormalizeRequiredValue(
                dto.IssueCategory,
                "Issue category is required.");

            var vehicleBrand = NormalizeOptionalValue(dto.VehicleBrand);

            center.Capabilities ??= new List<CenterCapability>();

            var capabilityAlreadyExists = center.Capabilities.Any(capability =>
                capability.IssueCategory == issueCategory &&
                capability.VehicleBrand == vehicleBrand);

            if (capabilityAlreadyExists)
                throw new BadRequestException(
                    "This capability already exists for this service center.");

            var capability = new CenterCapability
            {
                Id = Guid.NewGuid(),
                ServiceCenterId = center.Id,
                IssueCategory = issueCategory,
                VehicleBrand = vehicleBrand
            };

            await _repository.AddCapabilityAsync(capability);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> UpdateCapabilityAsync(
            Guid ownerUserId,
            Guid capabilityId,
            UpdateCenterCapabilityDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);

            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            var capability = center.Capabilities
                .FirstOrDefault(item => item.Id == capabilityId);

            if (capability == null)
                throw new NotFoundException("Capability not found.");

            var issueCategory = NormalizeRequiredValue(
                dto.IssueCategory,
                "Issue category is required.");

            var vehicleBrand = NormalizeOptionalValue(dto.VehicleBrand);

            var capabilityAlreadyExists = center.Capabilities.Any(item =>
                item.Id != capabilityId &&
                item.IssueCategory == issueCategory &&
                item.VehicleBrand == vehicleBrand);

            if (capabilityAlreadyExists)
                throw new BadRequestException(
                    "This capability already exists for this service center.");

            capability.IssueCategory = issueCategory;
            capability.VehicleBrand = vehicleBrand;

            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> RemoveCapabilityAsync(
            Guid ownerUserId,
            Guid capabilityId)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);

            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            var capability = center.Capabilities
                .FirstOrDefault(item => item.Id == capabilityId);

            if (capability == null)
                throw new NotFoundException("Capability not found.");

            _repository.RemoveCapability(capability);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterPublicProfileResponseDto> GetPublicProfileAsync(
            Guid serviceCenterId)
        {
            var serviceCenter = await _repository.GetByIdAsync(serviceCenterId);

            if (serviceCenter == null ||
                serviceCenter.IsDeleted ||
                serviceCenter.VerificationStatus != VerificationStatus.Approved)
            {
                throw new NotFoundException("Service center not found.");
            }

            return new CenterPublicProfileResponseDto
            {
                Id = serviceCenter.Id,
                Name = serviceCenter.Name,
                Address = serviceCenter.Address,
                Phone = serviceCenter.Phone,
                Rating = serviceCenter.Rating,
                Capabilities = MapCapabilities(serviceCenter.Capabilities)
            };
        }

        private static CenterProfileResponseDto MapToProfileDto(ServiceCenter center)
        {
            return new CenterProfileResponseDto
            {
                Id = center.Id,
                Name = center.Name,
                Address = center.Address,
                Phone = center.Phone,
                Rating = center.Rating,
                VerificationStatus = center.VerificationStatus.ToString(),
                Capabilities = MapCapabilities(center.Capabilities)
            };
        }

        private static List<CenterCapabilityResponseDto> MapCapabilities(
            ICollection<CenterCapability>? capabilities)
        {
            return capabilities?
                .Select(capability => new CenterCapabilityResponseDto
                {
                    Id = capability.Id,
                    IssueCategory = capability.IssueCategory,
                    VehicleBrand = capability.VehicleBrand
                })
                .ToList()
                ?? new List<CenterCapabilityResponseDto>();
        }

        private static string NormalizeRequiredValue(
            string? value,
            string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException(errorMessage);

            return value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }
    }
}