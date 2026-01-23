using FluentValidation;

namespace IBTS2026.Application.Features.Users.UpdateUser
{
    public sealed class UpdateUserCommandValidator
        : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than zero.");

            When(x => x.FirstName is not null, () =>
            {
                RuleFor(x => x.FirstName!)
                    .NotEmpty().WithMessage("FirstName cannot be empty.")
                    .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");
            });
            When(x => x.LastName is not null, () =>
            {
                RuleFor(x => x.LastName!)
                    .NotEmpty().WithMessage("LastName cannot be empty.")
                    .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");
            });
            When(x => x.Role is not null, () =>
            {
                RuleFor(x => x.Role!)
                    .NotEmpty().WithMessage("Role cannot be empty.")
                    .MaximumLength(20).WithMessage("Role cannot exceed 20 characters.");
            });
        }
    }
}
