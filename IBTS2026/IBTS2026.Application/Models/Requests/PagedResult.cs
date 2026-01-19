namespace IBTS2026.Application.Models.Requests
{
    public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);

}
