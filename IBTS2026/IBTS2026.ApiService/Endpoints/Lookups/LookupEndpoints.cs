using IBTS2026.Application.Dtos.Lookups;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.ApiService.Endpoints.Lookups;

public static class LookupEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/priorities", async (IBTS2026Context context, CancellationToken ct) =>
        {
            var priorities = await context.Priorities
                .AsNoTracking()
                .OrderBy(p => p.PriorityId)
                .Select(p => new PriorityDto(p.PriorityId, p.PriorityName))
                .ToListAsync(ct);

            return Results.Ok(priorities);
        })
        .RequireAuthorization("RequireUserRole")
        .WithName("GetPriorities")
        .WithSummary("Get all priorities")
        .WithDescription("Retrieves all available priority levels for incidents.")
        .WithTags("Lookups")
        .Produces<List<PriorityDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/statuses", async (IBTS2026Context context, CancellationToken ct) =>
        {
            var statuses = await context.Statuses
                .AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new StatusDto(s.StatusId, s.StatusName))
                .ToListAsync(ct);

            return Results.Ok(statuses);
        })
        .RequireAuthorization("RequireUserRole")
        .WithName("GetStatuses")
        .WithSummary("Get all statuses")
        .WithDescription("Retrieves all available status values for incidents.")
        .WithTags("Lookups")
        .Produces<List<StatusDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
