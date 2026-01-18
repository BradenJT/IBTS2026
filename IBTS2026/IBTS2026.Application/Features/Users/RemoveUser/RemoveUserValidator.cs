using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IBTS2026.Application.Features.Users.RemoveUser
{
    internal class RemoveUserValidator
        : AbstractValidator<RemoveUserCommand>
    {
        public RemoveUserValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than zero.");
        }
    }
}
