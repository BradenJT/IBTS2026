using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Interfaces.Incidents;

namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    public sealed class UpdateIncidentHandler(
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        IValidator<UpdateIncidentCommand> validator) : IRequestHandler<UpdateIncidentCommand, bool>
    {
        private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IValidator<UpdateIncidentCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(UpdateIncidentCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var incident = await _incidents.GetByIdAsync(command.IncidentId, ct);

            if (incident is null)
            {
                return false;
            }

            incident.ChangeTitle(command.Title);
            incident.ChangeDescription(command.Description);
            incident.ChangeStatus(command.StatusId);
            incident.ChangePriority(command.PriorityId);
            incident.AssignTo(command.AssignedToUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
