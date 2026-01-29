using IBTS2026.Application.Abstractions.Services;
using Microsoft.AspNetCore.Identity;

namespace IBTS2026.Infrastructure.Services;

internal sealed class PasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
