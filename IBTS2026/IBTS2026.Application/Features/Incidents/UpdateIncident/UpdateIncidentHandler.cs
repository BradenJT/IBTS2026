using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    public sealed class UpdateIncidentHandler(
        IIncidentRepository incidents,
        IUserRepository users,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IValidator<UpdateIncidentCommand> validator) : IRequestHandler<UpdateIncidentCommand, bool>
    {
        private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
        private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IValidator<UpdateIncidentCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(UpdateIncidentCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var incident = await _incidents.GetByIdWithDetailsAsync(command.IncidentId, ct);

            if (incident is null)
            {
                return false;
            }

            // Capture original values for change detection
            var originalStatusId = incident.StatusId;
            var originalPriorityId = incident.PriorityId;
            var originalAssignedTo = incident.AssignedTo;
            var originalStatusName = incident.Status?.StatusName ?? "Unknown";
            var originalPriorityName = incident.Priority?.PriorityName ?? "Unknown";

            // Apply changes
            incident.ChangeTitle(command.Title);
            incident.ChangeDescription(command.Description);
            incident.ChangeStatus(command.StatusId);
            incident.ChangePriority(command.PriorityId);
            incident.AssignTo(command.AssignedToUserId);

            // For notifications, we need to know who made the change
            // In a real app, this would come from the current user context
            // For now, we'll use the incident creator as the change initiator
            var changedByUser = incident.CreatedByUser;

            // Queue notifications for changes
            if (originalAssignedTo != command.AssignedToUserId && command.AssignedToUserId.HasValue)
            {
                var assignedUser = await _users.GetByIdAsync(command.AssignedToUserId.Value, ct);
                if (assignedUser != null)
                {
                    _notificationService.QueueAssignmentNotification(incident, assignedUser);
                }
            }

            if (originalStatusId != command.StatusId && changedByUser != null)
            {
                // We need the new status name - load it from the lookup table
                var newStatusName = command.StatusId switch
                {
                    1 => "Open",
                    2 => "In Progress",
                    3 => "Closed",
                    4 => "Unknown",
                    _ => "Unknown"
                };
                _notificationService.QueueStatusChangeNotification(incident, originalStatusName, newStatusName, changedByUser);
            }

            if (originalPriorityId != command.PriorityId && changedByUser != null)
            {
                // We need the new priority name
                var newPriorityName = command.PriorityId switch
                {
                    1 => "Low",
                    2 => "Medium",
                    3 => "High",
                    4 => "Critical",
                    _ => "Unknown"
                };
                _notificationService.QueuePriorityChangeNotification(incident, originalPriorityName, newPriorityName, changedByUser);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
