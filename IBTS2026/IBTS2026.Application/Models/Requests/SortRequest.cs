namespace IBTS2026.Application.Models.Requests
{
    public sealed record SortRequest(
        string Field,
        SortDirection Direction);

    public enum SortDirection
    {
        Asc,
        Desc
    }
}
