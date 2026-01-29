using System.Security.Claims;

namespace IBTS2026.Application.Abstractions.Services;

public interface ITokenService
{
    string GenerateToken(int userId, string email, string role);
    ClaimsPrincipal? ValidateToken(string token);
}
