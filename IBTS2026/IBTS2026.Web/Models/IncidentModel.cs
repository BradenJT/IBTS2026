namespace IBTS2026.Web.Models
{
    public sealed record IncidentModel(
        int IncidentId,
        string Title,
        int StatusId,
        string StatusName,
        int PriorityId,
        string PriorityName,
        int? AssignedTo,
        DateTime CreatedAt);

    public sealed record IncidentDetailsModel(
        int IncidentId,
        string Title,
        string Description,
        int StatusId,
        string StatusName,
        int PriorityId,
        string PriorityName,
        int CreatedBy,
        int? AssignedTo,
        DateTime CreatedAt);

    public sealed record CreateIncidentModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusId { get; set; } = 1; // Default to Open
        public int PriorityId { get; set; } = 1;
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public sealed record UpdateIncidentModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public int PriorityId { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
