using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Incidents.IncidentNote;
using IBTS2026.Domain.Interfaces.IncidentNotes;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;

public sealed class CreateIncidentNoteHandler(
    IIncidentNoteRepository incidentNotes,
    IIncidentRepository incidents,
    IUserRepository users,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    IValidator<CreateIncidentNoteCommand> validator) : IRequestHandler<CreateIncidentNoteCommand, int>
{
    private readonly IIncidentNoteRepository _incidentNotes = incidentNotes ?? throw new ArgumentNullException(nameof(incidentNotes));
    private readonly IIncidentRepository _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IValidator<CreateIncidentNoteCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(
        CreateIncidentNoteCommand command,
        CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var incident = await _incidents.GetByIdAsync(command.IncidentId, ct)
            ?? throw new InvalidOperationException($"Incident with ID {command.IncidentId} not found.");

        var noteAuthor = await _users.GetByIdAsync(command.CreatedByUserId, ct)
            ?? throw new InvalidOperationException($"User with ID {command.CreatedByUserId} not found.");

        var incidentCreator = await _users.GetByIdAsync(incident.CreatedBy, ct);

        var note = IncidentNote.Create(
            command.IncidentId,
            command.CreatedByUserId,
            command.Content);

        _incidentNotes.Add(note);

        // Queue notification if the note author is different from the incident creator
        if (incidentCreator != null && noteAuthor.UserId != incidentCreator.UserId)
        {
            _notificationService.QueueNoteAddedNotification(incident, noteAuthor, incidentCreator);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return note.IncidentNoteId;
    }
}
