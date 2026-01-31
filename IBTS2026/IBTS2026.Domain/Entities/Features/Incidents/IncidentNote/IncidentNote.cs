#nullable enable

using IncidentEntity = IBTS2026.Domain.Entities.Features.Incidents.Incident.Incident;
using IBTS2026.Domain.Entities.Features.Users;

namespace IBTS2026.Domain.Entities.Features.Incidents.IncidentNote;

public class IncidentNote
{
    public int IncidentNoteId { get; set; }
    public int IncidentId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual IncidentEntity Incident { get; set; } = null!;
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
