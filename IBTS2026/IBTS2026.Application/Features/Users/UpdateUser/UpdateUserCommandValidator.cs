using FluentValidation;

namespace IBTS2026.Application.Features.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private static readonly string[] ValidRoles = ["Admin", "User"];

    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than zero.");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0).WithMessage("CurrentUserId must be greater than zero.");

        RuleFor(x => x.CurrentUserRole)
            .NotEmpty().WithMessage("CurrentUserRole is required.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(250).WithMessage("Email cannot exceed 250 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName!)
                .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName!)
                .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Role), () =>
        {
            RuleFor(x => x.Role!)
                .Must(role => ValidRoles.Contains(role)).WithMessage("Role must be either 'Admin' or 'User'.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
        {
            RuleFor(x => x.NewPassword!)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        });
    }
}

