using FluentValidation;

namespace IBTS2026.Application.Features.Users.UpdateUser
{
    internal class UpdateUserValidator
        : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be null or empty.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName cannot be null or empty.");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName cannot be null or empty.");
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role cannot be null or empty.");
        }
    }
}
