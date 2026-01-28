#nullable enable

namespace IBTS2026.Domain.Entities;

public class IncidentNote
{
    public int IncidentNoteId { get; set; }
    public int IncidentId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Incident Incident { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;

    public static IncidentNote Create(
        int incidentId,
        int createdByUserId,
        string content)
    {
        return new IncidentNote
        {
            IncidentId = incidentId,
            CreatedByUserId = createdByUserId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
    }
}
