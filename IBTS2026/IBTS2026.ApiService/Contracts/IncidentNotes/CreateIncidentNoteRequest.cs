namespace IBTS2026.Api.Contracts.IncidentNotes;

public sealed class CreateIncidentNoteRequest
{
    public int CreatedByUserId { get; init; }
    public string Content { get; init; } = string.Empty;
}
