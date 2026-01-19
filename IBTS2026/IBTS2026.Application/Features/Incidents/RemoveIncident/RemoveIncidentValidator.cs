using FluentValidation;

namespace IBTS2026.Application.Features.Incidents.RemoveIncident
{
    internal sealed class RemoveIncidentValidator
        : AbstractValidator<RemoveIncidentCommand>
    {
        public RemoveIncidentValidator()
        {
            RuleFor(x => x.IncidentId)
                .GreaterThan(0).WithMessage("IncidentId must be greater than zero");
        }
    }
}
