namespace IBTS2026.Web.Models.Auth;

public sealed record RegisterResultModel(
    int UserId,
    string Email,
    string Role
);
