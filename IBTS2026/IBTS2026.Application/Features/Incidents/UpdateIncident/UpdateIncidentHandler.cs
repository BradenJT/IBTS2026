using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Incidents;

namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    internal class UpdateIncidentHandler(
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        IValidator<UpdateIncidentCommand> validator) : IRequestHandler<UpdateIncidentCommand, bool>
    {
        private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(_unitOfWork));
        private readonly IValidator<UpdateIncidentCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(UpdateIncidentCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var incident = await _incidents.GetByIdAsync(command.IncidentId, ct);

            if (incident == null)
            {
                return false;
            }

            if (HasChanges(incident, command))
            {
                Incident.Update(incident, command.Title, command.Description, command.StatusId, command.PriorityId, command.AssignedToUserId);
            }

            _incidents.Update(incident);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static bool HasChanges(Incident incident, UpdateIncidentCommand command) =>
            incident.Title != command.Title ||
            incident.Description != command.Description ||
            incident.StatusId != command.StatusId ||
            incident.PriorityId != command.PriorityId ||
            incident.AssignedTo != command.AssignedToUserId;

    }
}
