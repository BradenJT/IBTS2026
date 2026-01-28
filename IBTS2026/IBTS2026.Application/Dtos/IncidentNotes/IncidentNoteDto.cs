namespace IBTS2026.Application.Dtos.IncidentNotes;

public sealed record IncidentNoteDto(
    int IncidentNoteId,
    int IncidentId,
    int CreatedByUserId,
    string CreatedByName,
    string Content,
    DateTime CreatedAt);
