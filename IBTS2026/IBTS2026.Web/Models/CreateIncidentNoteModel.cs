namespace IBTS2026.Web.Models;

public sealed class CreateIncidentNoteModel
{
    public int CreatedByUserId { get; set; }
    public string Content { get; set; } = string.Empty;
}
