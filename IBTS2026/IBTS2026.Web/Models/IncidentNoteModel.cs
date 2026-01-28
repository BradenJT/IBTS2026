namespace IBTS2026.Web.Models;

public sealed record IncidentNoteModel(
    int IncidentNoteId,
    int IncidentId,
    int CreatedByUserId,
    string CreatedByName,
    string Content,
    DateTime CreatedAt);
