using IBTS2026.Application.Dtos.Lookups;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.ApiService.Controllers.Lookups
{
    [ApiController]
    [Route("")]
    [Authorize(Policy = "RequireUserRole")]
    [Produces("application/json")]
    public class LookupsController : ControllerBase
    {
        private readonly IBTS2026Context _context;

        public LookupsController(IBTS2026Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all priorities
        /// </summary>
        /// <remarks>
        /// Retrieves all available priority levels for incidents.
        /// </remarks>
        [HttpGet("priorities")]
        [ProducesResponseType(typeof(List<PriorityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPriorities(CancellationToken ct)
        {
            var priorities = await _context.Priorities
                .AsNoTracking()
                .OrderBy(p => p.PriorityId)
                .Select(p => new PriorityDto(p.PriorityId, p.PriorityName))
                .ToListAsync(ct);

            return Ok(priorities);
        }

        /// <summary>
        /// Get all statuses
        /// </summary>
        /// <remarks>
        /// Retrieves all available status values for incidents.
        /// </remarks>
        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<StatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStatuses(CancellationToken ct)
        {
            var statuses = await _context.Statuses
                .AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new StatusDto(s.StatusId, s.StatusName))
                .ToListAsync(ct);

            return Ok(statuses);
        }
    }
}
