using FluentValidation;
using IBTS2026.Domain.Enums;

namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    internal sealed class UpdateIncidentCommandValidator
        : AbstractValidator<UpdateIncidentCommand>
    {
        public UpdateIncidentCommandValidator()
        {
            RuleFor(x => x.IncidentId)
                .GreaterThan(0).WithMessage("IncidentId must be greater than zero");

            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title!)
                    .NotEmpty().WithMessage("Title cannot be empty")
                    .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.");
            });
            When(x => x.Description is not null, () =>
            {
                RuleFor(x => x.Description!)
                    .NotEmpty().WithMessage("Description cannot be empty");
            });
            When(x => x.StatusId > 0, () =>
            {
                RuleFor(x => x.StatusId!)
                    .GreaterThan(0).WithMessage("StatusId must be greater than zero")
                    .Must(value => Enum.IsDefined(typeof(IncidentStatus), value))
                    .WithMessage("StatusId must be a valid IncidentStatus value.");
            });
            When(x => x.PriorityId > 0, () =>
            {
                RuleFor(x => x.PriorityId!)
                   .GreaterThan(0).WithMessage("PriorityId must be greater than zero")
                   .Must(value => Enum.IsDefined(typeof(PriorityStatus), value))
                   .WithMessage("PriorityId must be a valid PriorityStatus value.");
            });
        }
    }
}
