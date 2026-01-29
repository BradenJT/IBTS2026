using IBTS2026.Api.Contracts.IncidentNotes;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.IncidentNotes;
using IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;
using IBTS2026.Application.Features.IncidentNotes.GetIncidentNotes;

namespace IBTS2026.ApiService.Endpoints.IncidentNotes;

public static class IncidentNoteEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/incidents/{incidentId:int}/notes", async (
            int incidentId,
            IRequestDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var result = await dispatcher
                .SendAsync<GetIncidentNotesQuery, IReadOnlyList<IncidentNoteDto>>(
                    new GetIncidentNotesQuery(incidentId), ct);

            return Results.Ok(result);
        })
        .RequireAuthorization("RequireUserRole")
        .WithName("GetIncidentNotes")
        .WithSummary("Get notes for an incident")
        .WithDescription("Retrieves all journal notes for a specific incident, ordered by creation date descending.")
        .WithTags("Incident Notes")
        .Produces<IReadOnlyList<IncidentNoteDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        app.MapPost("/incidents/{incidentId:int}/notes", async (
            int incidentId,
            CreateIncidentNoteRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new CreateIncidentNoteCommand(
                incidentId,
                request.CreatedByUserId,
                request.Content);

            var noteId = await dispatcher
                .SendAsync<CreateIncidentNoteCommand, int>(command, ct);

            return Results.Created($"/incidents/{incidentId}/notes/{noteId}", noteId);
        })
        .RequireAuthorization("RequireUserRole")
        .WithName("CreateIncidentNote")
        .WithSummary("Add a note to an incident")
        .WithDescription("Creates a new journal note for the specified incident. Returns the ID of the created note.")
        .WithTags("Incident Notes")
        .Produces<int>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
