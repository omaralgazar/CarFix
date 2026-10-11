using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Validators.Common;
using FluentValidation;

namespace CarFix.Application.Validators.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().MaximumLength(AuthRules.EmailMaxLength);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(AuthRules.PasswordMaxLength);
        }
    }
}