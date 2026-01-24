namespace IBTS2026.Api.Contracts.Incidents
{
    public sealed class UpdateIncidentRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int StatusId { get; init; }
        public int PriorityId { get; init; }
        public int? AssignedToUserId { get; init; }
    }
}
