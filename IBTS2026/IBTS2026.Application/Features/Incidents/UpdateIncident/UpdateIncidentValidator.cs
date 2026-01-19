using FluentValidation;
using IBTS2026.Domain.Enums;

namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    internal class UpdateIncidentValidator
        : AbstractValidator<UpdateIncidentCommand>
    {
       public UpdateIncidentValidator()
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
            RuleFor(x => x.AssignedToUserId)
                .GreaterThan(0).WithMessage("AssignedToUserid must be greater than zero");
        }
    }
}
