using CarFix.Application.DTOs.AuthDto;
using CarFix.Application.Validators.Common;
using FluentValidation;

namespace CarFix.Application.Validators.Auth
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Token).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NewPassword).MustBeStrongPassword();
            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("New password and confirmation password do not match.");
        }
    }
}