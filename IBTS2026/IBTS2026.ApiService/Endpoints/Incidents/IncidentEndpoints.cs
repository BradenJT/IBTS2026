using IBTS2026.Api.Contracts.Incidents;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Features.Incidents.CreateIncident;
using IBTS2026.Application.Features.Incidents.GetIncident;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Features.Incidents.RemoveIncident;
using IBTS2026.Application.Features.Incidents.UpdateIncident;
using IBTS2026.Application.Models.Requests;

namespace IBTS2026.ApiService.Endpoints.Incidents
{
    public static class IncidentEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/incidents/{id:int}", async (
                int id,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var result = await dispatcher
                    .SendAsync<GetIncidentQuery, IncidentDetailsDto?>(
                        new GetIncidentQuery(id), ct);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            })
            .WithName("GetIncident")
            .WithSummary("Get an incident by ID")
            .WithDescription("Retrieves detailed information about a specific incident by its unique identifier.")
            .WithTags("Incidents")
            .Produces<IncidentDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapGet("/incidents", async (
                int? pageNumber,
                int? pageSize,
                string? search,
                string? sortBy,
                string? sortDir,
                int? statusId,
                int? priorityId,
                int? assignedToUserId,
                DateTime? createdAfter,
                DateTime? createdBefore,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var query = new GetIncidentsQuery(
                    new PageRequest(pageNumber ?? 1, pageSize ?? 20),
                    sortBy is null
                        ? null
                        : new SortRequest(
                            sortBy,
                            sortDir == "desc" ? SortDirection.Desc : SortDirection.Asc),
                    search,
                    statusId,
                    priorityId,
                    assignedToUserId,
                    createdAfter,
                    createdBefore);

                var result = await dispatcher
                    .SendAsync<GetIncidentsQuery, PagedResult<IncidentDto>>(query, ct);

                return Results.Ok(result);
            })
            .WithName("GetIncidents")
            .WithSummary("Get a paginated list of incidents")
            .WithDescription("Retrieves a paginated list of incidents with optional filtering by status, priority, assignee, and date range. Supports search and sorting.")
            .WithTags("Incidents")
            .Produces<PagedResult<IncidentDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPost("/incidents", async (
                CreateIncidentRequest request,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new CreateIncidentCommand(
                    request.Title,
                    request.Description,
                    request.StatusId,
                    request.PriorityId,
                    request.CreatedByUserId,
                    request.AssignedToUserId);

                var incidentId = await dispatcher
                    .SendAsync<CreateIncidentCommand, int>(command, ct);

                return Results.Created($"/incidents/{incidentId}", incidentId);
            })
            .WithName("CreateIncident")
            .WithSummary("Create a new incident")
            .WithDescription("Creates a new incident with the provided details. Returns the ID of the created incident.")
            .WithTags("Incidents")
            .Produces<int>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPut("/incidents/{id:int}", async (
                int id,
                UpdateIncidentRequest request,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new UpdateIncidentCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.StatusId,
                    request.PriorityId,
                    0, // CreatedByUserId is not updated
                    request.AssignedToUserId);

                var result = await dispatcher
                    .SendAsync<UpdateIncidentCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateIncident")
            .WithSummary("Update an existing incident")
            .WithDescription("Updates an existing incident's details. Status transitions are validated according to business rules.")
            .WithTags("Incidents")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapDelete("/incidents/{id:int}", async (
                int id,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new RemoveIncidentCommand(id);

                var result = await dispatcher
                    .SendAsync<RemoveIncidentCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteIncident")
            .WithSummary("Delete an incident")
            .WithDescription("Deletes an incident by its unique identifier. Returns 204 No Content on success or 404 Not Found if the incident does not exist.")
            .WithTags("Incidents")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}
