using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Incidents.Incident;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Incidents.CreateIncident
{
    public sealed class CreateIncidentHandler(
        IIncidentRepository incidents,
        IUserRepository users,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IValidator<CreateIncidentCommand> validator) : IRequestHandler<CreateIncidentCommand, int>
    {
        private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
        private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IValidator<CreateIncidentCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<int> Handle(CreateIncidentCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var incident = Incident.Create(
                command.Title,
                command.Description,
                command.StatusId,
                command.PriorityId,
                command.CreatedByUserId,
                command.AssignedToUserId);

            _incidents.Add(incident);

            await _unitOfWork.SaveChangesAsync(ct);

            // Queue assignment notification if incident is created with an assignee
            if (command.AssignedToUserId.HasValue)
            {
                var assignedUser = await _users.GetByIdAsync(command.AssignedToUserId.Value, ct);
                if (assignedUser != null)
                {
                    // Reload incident with navigation properties for notification
                    var createdIncident = await _incidents.GetByIdWithDetailsAsync(incident.IncidentId, ct);
                    if (createdIncident != null)
                    {
                        _notificationService.QueueAssignmentNotification(createdIncident, assignedUser);
                        await _unitOfWork.SaveChangesAsync(ct);
                    }
                }
            }

            return incident.IncidentId;
        }
    }
}
