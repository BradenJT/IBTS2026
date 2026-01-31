using IBTS2026.Api.Contracts.Incidents;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Features.Incidents.CreateIncident;
using IBTS2026.Application.Features.Incidents.GetIncident;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Features.Incidents.RemoveIncident;
using IBTS2026.Application.Features.Incidents.UpdateIncident;
using IBTS2026.Application.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBTS2026.ApiService.Controllers.Incidents
{
    [ApiController]
    [Route("incidents")]
    [Authorize(Policy = "RequireUserRole")]
    [Produces("application/json")]
    public class IncidentsController : ControllerBase
    {
        private readonly IRequestDispatcher _dispatcher;

        public IncidentsController(IRequestDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Get an incident by ID
        /// </summary>
        /// <remarks>
        /// Retrieves detailed information about a specific incident by its unique identifier.
        /// </remarks>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(IncidentDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIncident(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _dispatcher
                .SendAsync<GetIncidentQuery, IncidentDetailsDto?>(
                    new GetIncidentQuery(id), ct);

            return result is null
                ? NotFound()
                : Ok(result);
        }

        /// <summary>
        /// Get a paginated list of incidents
        /// </summary>
        /// <remarks>
        /// Retrieves a paginated list of incidents with optional filtering by status, priority, assignee, and date range. 
        /// Supports search and sorting.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<IncidentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIncidents(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            [FromQuery] int? statusId,
            [FromQuery] int? priorityId,
            [FromQuery] int? assignedToUserId,
            [FromQuery] DateTime? createdAfter,
            [FromQuery] DateTime? createdBefore,
            CancellationToken ct)
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

            var result = await _dispatcher
                .SendAsync<GetIncidentsQuery, PagedResult<IncidentDto>>(query, ct);

            return Ok(result);
        }

        /// <summary>
        /// Create a new incident
        /// </summary>
        /// <remarks>
        /// Creates a new incident with the provided details. Returns the ID of the created incident.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateIncident(
            [FromBody] CreateIncidentRequest request,
            CancellationToken ct)
        {
            var command = new CreateIncidentCommand(
                request.Title,
                request.Description,
                request.StatusId,
                request.PriorityId,
                request.CreatedByUserId,
                request.AssignedToUserId);

            var incidentId = await _dispatcher
                .SendAsync<CreateIncidentCommand, int>(command, ct);

            return CreatedAtAction(
                nameof(GetIncident),
                new { id = incidentId },
                incidentId);
        }

        /// <summary>
        /// Update an existing incident
        /// </summary>
        /// <remarks>
        /// Updates an existing incident's details. Status transitions are validated according to business rules.
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateIncident(
            [FromRoute] int id,
            [FromBody] UpdateIncidentRequest request,
            CancellationToken ct)
        {
            var command = new UpdateIncidentCommand(
                id,
                request.Title,
                request.Description,
                request.StatusId,
                request.PriorityId,
                0, // CreatedByUserId is not updated
                request.AssignedToUserId);

            var result = await _dispatcher
                .SendAsync<UpdateIncidentCommand, bool>(command, ct);

            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Delete an incident
        /// </summary>
        /// <remarks>
        /// Deletes an incident by its unique identifier. Returns 204 No Content on success or 404 Not Found if the incident 
        /// does not exist. Requires Admin role.
        /// </remarks>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteIncident(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var command = new RemoveIncidentCommand(id);

            var result = await _dispatcher
                .SendAsync<RemoveIncidentCommand, bool>(command, ct);

            return result ? NoContent() : NotFound();
        }
    }
}
