using FluentValidation;

namespace IBTS2026.Application.Features.Users.CreateUser
{
    public sealed class CreateUserCommandValidator
        : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName cannot be empty.")
                .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName cannot be empty.")
                .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role cannot be empty.")
                .MaximumLength(20).WithMessage("Role cannot exceed 20 characters.");
        }
    }
}
