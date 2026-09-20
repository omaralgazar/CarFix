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

        public async Task<CenterProfileResponseDto> GetProfileAsync(Guid serviceCenterId)
        {
            var serviceCenter = await _repository.GetByIdAsync(serviceCenterId);
            if (serviceCenter == null || serviceCenter.IsDeleted)
                throw new NotFoundException("Service center not found.");

            return MapToProfileDto(serviceCenter);
        }

        public async Task<CenterProfileResponseDto> GetProfileByOwnerIdAsync(Guid ownerId)
        {
            var serviceCenter = await _repository.GetByOwnerIdAsync(ownerId);
            if (serviceCenter == null || serviceCenter.IsDeleted)
                throw new NotFoundException("Service center not found.");

            return MapToProfileDto(serviceCenter);
        }

        public async Task<CenterProfileResponseDto> UpdateProfileAsync(Guid ownerUserId, UpdateCenterProfileDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);
            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                center.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Address))
                center.Address = dto.Address;
            if (!string.IsNullOrWhiteSpace(dto.Phone))
                center.Phone = dto.Phone;

            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> AddSpecialtyAsync(Guid ownerUserId, AddSpecialtyDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);
            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            if (string.IsNullOrWhiteSpace(dto.Type) || string.IsNullOrWhiteSpace(dto.Value))
                throw new BadRequestException("Specialty type and value cannot be empty.");

            if (!Enum.TryParse<SpecialtyType>(dto.Type, true, out var specialtyType))
                throw new BadRequestException("Invalid specialty type.");

            if (center.Specialties.Any(s => s.Type == specialtyType && s.Value.Equals(dto.Value, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException("This specialty already exists for this center.");

            var specialty = new CenterSpecialty
            {
                Id = Guid.NewGuid(),
                Type = specialtyType,
                Value = dto.Value,
                ServiceCenterId = center.Id
            };

            center.Specialties ??= new List<CenterSpecialty>();
            center.Specialties.Add(specialty);
            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> UpdateSpecialtyAsync(Guid ownerUserId, Guid specialtyId, UpdateCenterSpecialityDto dto)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);
            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            var specialty = center.Specialties.FirstOrDefault(s => s.Id == specialtyId);
            if (specialty == null)
                throw new NotFoundException("Specialty not found.");

            if (!string.IsNullOrWhiteSpace(dto.Type))
            {
                if (!Enum.TryParse<SpecialtyType>(dto.Type, true, out var specialtyType))
                    throw new BadRequestException("Invalid specialty type.");

                specialty.Type = specialtyType;
            }

            if (!string.IsNullOrWhiteSpace(dto.Value))
                specialty.Value = dto.Value;

            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }

        public async Task<CenterProfileResponseDto> RemoveSpecialtyAsync(Guid ownerUserId, Guid specialtyId)
        {
            var center = await _repository.GetByOwnerIdAsync(ownerUserId);
            if (center == null || center.IsDeleted)
                throw new NotFoundException("Service center not found.");

            var specialty = center.Specialties.FirstOrDefault(s => s.Id == specialtyId);
            if (specialty == null)
                throw new NotFoundException("Specialty not found.");

            center.Specialties.Remove(specialty);
            _repository.Update(center);
            await _repository.SaveChangesAsync();

            return MapToProfileDto(center);
        }
        public async Task<CenterPublicProfileResponseDto> GetPublicProfileAsync(Guid serviceCenterId)
        {
            var serviceCenter = await _repository.GetByIdAsync(serviceCenterId);

            if (serviceCenter == null || serviceCenter.IsDeleted || serviceCenter.VerificationStatus != VerificationStatus.Approved)
                throw new NotFoundException("Service center not found.");

            return new CenterPublicProfileResponseDto
            {
                Id = serviceCenter.Id,
                Name = serviceCenter.Name,
                Address = serviceCenter.Address,
                Phone = serviceCenter.Phone,
                Rating = serviceCenter.Rating,
                Specialties = serviceCenter.Specialties.Select(s => new CenterSpecialtyResponseDto
                {
                    Id = s.Id,
                    Type = s.Type.ToString(),
                    Value = s.Value
                }).ToList()
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
                Specialties = center.Specialties?.Select(s => new CenterSpecialtyResponseDto
                {
                    Id = s.Id,
                    Type = s.Type.ToString(),
                    Value = s.Value
                }).ToList() ?? new List<CenterSpecialtyResponseDto>()
            };
        }
    }
}