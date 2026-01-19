using FluentValidation;

namespace IBTS2026.Application.Features.Users.RemoveUser
{
    internal sealed class RemoveUserCommandValidator
        : AbstractValidator<RemoveUserCommand>
    {
        public RemoveUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than zero.");
        }
    }
}
