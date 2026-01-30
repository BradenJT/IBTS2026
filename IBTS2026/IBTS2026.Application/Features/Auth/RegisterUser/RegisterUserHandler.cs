using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Auth.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHashingService passwordHasher,
    IValidator<RegisterUserCommand> validator) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPasswordHashingService _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IValidator<RegisterUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new RegisterUserResult(false, null, null, null, errors);
        }

        // Check if email is already taken
        var existingUser = await _users.GetByEmailAsync(command.Email, ct);
        if (existingUser is not null)
        {
            return new RegisterUserResult(false, null, null, null, "Email is already registered.");
        }

        // Determine role: first user becomes Admin
        var anyUsersExist = await _users.AnyUsersExistAsync(ct);
        var role = anyUsersExist ? "User" : "Admin";

        // Hash password and create user
        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var user = User.CreateWithPassword(
            command.Email,
            command.FirstName,
            command.LastName,
            role,
            passwordHash);

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return new RegisterUserResult(true, user.UserId, user.Email, user.Role, null);
    }
}
