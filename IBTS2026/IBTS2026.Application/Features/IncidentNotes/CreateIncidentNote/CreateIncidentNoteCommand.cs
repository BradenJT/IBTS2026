namespace IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;

public sealed record CreateIncidentNoteCommand(
    int IncidentId,
    int CreatedByUserId,
    string Content);
