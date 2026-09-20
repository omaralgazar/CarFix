using CarFix.Application.DTOs.Customer.Vehicle;
using FluentValidation;

namespace CarFix.Application.Validators.Customer.Vehicle
{
    public class CreateVehicleDtoValidator : AbstractValidator<CreateVehicleDto>
    {
        public CreateVehicleDtoValidator()
        {
            RuleFor(v => v.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .MaximumLength(100);

            RuleFor(v => v.Model)
                .NotEmpty().WithMessage("Model is required.")
                .MaximumLength(100);

            RuleFor(v => v.Year)
                .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

            RuleFor(v => v.LicensePlate)
                .NotEmpty().WithMessage("License plate is required.")
                .MaximumLength(20);
        }
    }
}