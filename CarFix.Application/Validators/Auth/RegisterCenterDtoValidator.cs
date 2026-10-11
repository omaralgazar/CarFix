using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Validators.Common;
using FluentValidation;

namespace CarFix.Application.Validators.Auth
{
    public class RegisterCenterDtoValidator : AbstractValidator<RegisterCenterDto>
    {
        public RegisterCenterDtoValidator()
        {
            RuleFor(x => x.Email).MustBeValidEmail();
            RuleFor(x => x.Password).MustBeStrongPassword();
            RuleFor(x => x.Password)
                .Must((dto, password) => !AuthRules.ContainsEmailName(dto.Email, password))
                .WithMessage("Password must not contain your email name.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        }
    }
}