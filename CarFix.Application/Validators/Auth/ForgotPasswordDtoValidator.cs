using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Validators.Common;
using FluentValidation;

namespace CarFix.Application.Validators.Auth
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email).MustBeValidEmail();
        }
    }
}