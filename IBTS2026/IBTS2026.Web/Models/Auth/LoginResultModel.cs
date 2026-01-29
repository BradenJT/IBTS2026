namespace IBTS2026.Web.Models.Auth;

public sealed record LoginResultModel(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token
);
