using FluentValidation;

namespace CarFix.Application.Validators.Common
{
    public static class AuthRules
    {
        public const int PasswordMinLength = 10;
        public const int PasswordMaxLength = 128;
        public const int EmailMaxLength = 200;

        // قايمة صغيرة للتجربة. للأفضل استبدلها بقايمة أكبر (مثلاً top 10k من SecLists)
        private static readonly HashSet<string> CommonPasswords =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "1234567890", "0123456789", "123456789a", "qwertyuiop", "1q2w3e4r5t",
                "password12", "password123", "password1234", "qwerty123", "qwerty12345",
                "adminadmin", "admin12345", "iloveyou12", "welcome123", "letmein123",
                "carfix1234", "carfix12345"
            };

        public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(
            this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(PasswordMinLength)
                    .WithMessage($"Password must be at least {PasswordMinLength} characters.")
                .MaximumLength(PasswordMaxLength)
                    .WithMessage($"Password must not exceed {PasswordMaxLength} characters.")
                .Must(p => p is null || p.Distinct().Count() >= 5)
                    .WithMessage("Password is too repetitive.")
                .Must(p => p is null || !CommonPasswords.Contains(p))
                    .WithMessage("Password is too common.");
        }

        public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(
            this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(EmailMaxLength)
                    .WithMessage($"Email must not exceed {EmailMaxLength} characters.")
                .EmailAddress().WithMessage("Email format is invalid.");
        }

        public static bool ContainsEmailName(string? email, string? password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                return false;

            var localPart = email.Split('@')[0];

            return localPart.Length >= 4 &&
                   password.Contains(localPart, StringComparison.OrdinalIgnoreCase);
        }
    }
}