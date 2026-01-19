using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Interfaces.Incidents;

namespace IBTS2026.Application.Features.Incidents.RemoveIncident
{
    public sealed class RemoveIncidentHandler(
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        IValidator<RemoveIncidentCommand> validator) : IRequestHandler<RemoveIncidentCommand, bool>
    {
        private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(_unitOfWork));
        private readonly IValidator<RemoveIncidentCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(RemoveIncidentCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var incident = await _incidents.GetByIdAsync(command.IncidentId, ct);

            if (incident == null)
            {
                return false;
            }

            _incidents.Remove(incident);
            
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
