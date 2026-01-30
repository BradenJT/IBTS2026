namespace IBTS2026.Application.Dtos.Users;

/// <summary>
/// Simplified user DTO for dropdown/lookup purposes.
/// Contains only the minimal information needed for selection.
/// </summary>
public sealed record UserLookupDto(
    int UserId,
    string FullName
);
