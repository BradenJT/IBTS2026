using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Interfaces.Incidents;

namespace IBTS2026.Application.Features.Incidents.CreateIncident
{
    public sealed class CreateIncidentHandler(
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        IValidator<CreateIncidentCommand> validator) : IRequestHandler<CreateIncidentCommand, int>
    {
        public async Task<int> Handle(CreateIncidentCommand request, CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);

            var incident = 
        }
    }
}
