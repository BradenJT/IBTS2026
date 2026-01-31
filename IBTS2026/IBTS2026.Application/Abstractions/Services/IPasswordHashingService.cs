namespace IBTS2026.Application.Abstractions.Services;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
