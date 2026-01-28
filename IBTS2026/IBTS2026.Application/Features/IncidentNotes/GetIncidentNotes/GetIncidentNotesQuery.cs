using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.IncidentNotes;

namespace IBTS2026.Application.Features.IncidentNotes.GetIncidentNotes;

public sealed record GetIncidentNotesQuery(int IncidentId) : IQuery<IReadOnlyList<IncidentNoteDto>>;
