using FluentValidation;

namespace IBTS2026.Application.Features.Incidents.RemoveIncident
{
    public sealed class RemoveIncidentCommandValidator
        :AbstractValidator<RemoveIncidentCommand>
    {
        public RemoveIncidentCommandValidator()
        {
            RuleFor(x => x.IncidentId)
                .GreaterThan(0).WithMessage("UserId must be greater than zero");
        }
    }
}
