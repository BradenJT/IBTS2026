using FluentValidation;
using IBTS2026.Domain.Enums;

namespace IBTS2026.Application.Features.Incidents.CreateIncident
{
    internal class CreateIncidentValidator
            : AbstractValidator<CreateIncidentCommand>
    {
        public CreateIncidentValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be null or emtpy");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Message cannot be null or empty");
            RuleFor(x => x.StatusId)
                .NotEmpty().WithMessage("StatusId cannot be null or emtpy")
                .Must(value => Enum.IsDefined(typeof(IncidentStatus), value))
                .WithMessage("StatusId must be a valid IncidentStatus value.");
            RuleFor(x => x.CreatedByUserId)
                .GreaterThan(0).WithMessage("CreatedByUserId must be greater than 0");
            RuleFor(x => x.CreatedAt)
                .GreaterThan(DateTime.MinValue).WithMessage("CreatedAt must be higher than MinValue")
                .LessThan(DateTime.MaxValue).WithMessage("CreatedAt must be lower than MaxValue");
        }
    }
}
