using FluentValidation;

namespace IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;

public sealed class CreateIncidentNoteCommandValidator
    : AbstractValidator<CreateIncidentNoteCommand>
{
    public CreateIncidentNoteCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .GreaterThan(0).WithMessage("IncidentId must be greater than 0.");

        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage("CreatedByUserId must be greater than 0.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content cannot be empty.")
            .MaximumLength(4000).WithMessage("Content cannot exceed 4000 characters.");
    }
}
