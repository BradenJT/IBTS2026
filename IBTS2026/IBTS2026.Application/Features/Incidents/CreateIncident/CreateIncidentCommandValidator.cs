using FluentValidation;

namespace IBTS2026.Application.Features.Incidents.CreateIncident
{
    internal sealed class CreateIncidentCommandValidator
        : AbstractValidator<CreateIncidentCommand>
    {
        public CreateIncidentCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.StatusId)
                .GreaterThan(0).WithMessage("StatusId must be greater than zero.");
            RuleFor(x => x.PriorityId)
                .GreaterThan(0).WithMessage("PriorityId must be greater than zero.");
            RuleFor(x => x.CreatedByUserId)
                .GreaterThan(0).WithMessage("CreatedByUserId must be greater than zero.");
            RuleFor(x => x.AssignedToUserId)
                .GreaterThan(0).WithMessage("AssignedToUserId must be greater than zero.");
            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CreatedAt cannot be in the future.");
        }
    }
}
