using IBTS2026.Api.Contracts.IncidentNotes;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.IncidentNotes;
using IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;
using IBTS2026.Application.Features.IncidentNotes.GetIncidentNotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBTS2026.ApiService.Controllers.IncidentNotes
{
    [ApiController]
    [Route("incidents")]
    [Authorize(Policy = "RequireUserRole")]
    [Produces("application/json")]
    public class IncidentNotesController : ControllerBase
    {
        private readonly IRequestDispatcher _dispatcher;

        public IncidentNotesController(IRequestDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Get notes for an incident
        /// </summary>
        /// <remarks>
        /// Retrieves all journal notes for a specific incident, ordered by creation date descending.
        /// </remarks>
        [HttpGet("{incidentId:int}/notes")]
        [ProducesResponseType(typeof(IReadOnlyList<IncidentNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIncidentNotes(
            [FromRoute] int incidentId,
            CancellationToken ct)
        {
            var result = await _dispatcher
                .SendAsync<GetIncidentNotesQuery, IReadOnlyList<IncidentNoteDto>>(
                    new GetIncidentNotesQuery(incidentId), ct);

            return Ok(result);
        }

        /// <summary>
        /// Add a note to an incident
        /// </summary>
        /// <remarks>
        /// Creates a new journal note for the specified incident. Returns the ID of the created note.
        /// </remarks>
        [HttpPost("{incidentId:int}/notes")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateIncidentNote(
            [FromRoute] int incidentId,
            [FromBody] CreateIncidentNoteRequest request,
            CancellationToken ct)
        {
            var command = new CreateIncidentNoteCommand(
                incidentId,
                request.CreatedByUserId,
                request.Content);

            var noteId = await _dispatcher
                .SendAsync<CreateIncidentNoteCommand, int>(command, ct);

            return CreatedAtAction(
                nameof(GetIncidentNotes),
                new { incidentId },
                noteId);
        }
    }
}
