namespace IBTS2026.Domain.Enums
{
    /// <summary>
    /// Specifies the status of an incident.
    /// </summary>
    /// <remarks>Use this enumeration to represent the current state of an incident, such as whether it is
    /// open, in progress, closed, or unknown. The values can be used to control workflow or display status information
    /// in user interfaces.</remarks>
    public enum IncidentStatus
    {
        Open = 1,
        InProgress = 2,
        Closed = 3,
        Unknown = 4,
    }
}
