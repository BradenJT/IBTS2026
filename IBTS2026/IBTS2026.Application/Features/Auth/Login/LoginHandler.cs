using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Interfaces.Users;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHashingService passwordHasher,
    ITokenService tokenService,
    IValidator<LoginCommand> validator,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPasswordHashingService _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly IValidator<LoginCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<LoginHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new LoginResult(false, null, null, null, null, null, null, errors);
        }

        var user = await _users.GetByEmailAsync(command.Email, ct);
        if (user is null)
        {
            return new LoginResult(false, null, null, null, null, null, null, "Invalid email or password.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return new LoginResult(false, null, null, null, null, null, null, "Account is disabled.");
        }

        // Check if account is locked out
        if (user.IsLockedOut)
        {
            return new LoginResult(false, null, null, null, null, null, null, "Account is locked. Please try again later.");
        }

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !_passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
        {
            user.RecordLoginFailure();
            _users.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            if (user.IsLockedOut)
            {
                return new LoginResult(false, null, null, null, null, null, null, "Account is locked due to too many failed attempts. Please try again in 15 minutes.");
            }

            return new LoginResult(false, null, null, null, null, null, null, "Invalid email or password.");
        }

        // Successful login
        user.RecordLoginSuccess();
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "LOGIN DEBUG: User {UserId} ({Email}) logging in with Role: '{Role}'",
            user.UserId, user.Email, user.Role ?? "(null)");

        // Generate JWT token
        var token = _tokenService.GenerateToken(user.UserId, user.Email!, user.Role!);

        _logger.LogInformation(
            "LOGIN DEBUG: Generated token for user {UserId}, token length: {TokenLength}",
            user.UserId, token?.Length ?? 0);

        return new LoginResult(
            true,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token,
            null);
    }
}
