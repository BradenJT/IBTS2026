using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Users.UpdateUser;

public sealed class UpdateUserHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHashingService passwordHasher,
    IValidator<UpdateUserCommand> validator) : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPasswordHashingService _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IValidator<UpdateUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var user = await _users.GetByIdAsync(request.UserId, ct);

        if (user is null)
        {
            return false;
        }

        var isAdmin = string.Equals(request.CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);
        var isUpdatingSelf = request.CurrentUserId == request.UserId;

        // Non-admin users can only update themselves
        if (!isAdmin && !isUpdatingSelf)
        {
            throw new UnauthorizedAccessException("You can only update your own profile.");
        }

        // Non-admin users can only update FirstName, LastName, and Password
        if (!isAdmin)
        {
            // Non-admins cannot change email or role
            if (!string.IsNullOrWhiteSpace(request.Email) &&
                !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only administrators can change email addresses.");
            }

            if (!string.IsNullOrWhiteSpace(request.Role) &&
                !string.Equals(request.Role, user.Role, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only administrators can change user roles.");
            }
        }

        // Apply updates based on permissions
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.ChangeFirstName(request.FirstName);
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.ChangeLastName(request.LastName);
        }

        // Admin-only updates
        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Check if email is already taken by another user
                var existingUser = await _users.GetByEmailAsync(request.Email, ct);
                if (existingUser is not null && existingUser.UserId != request.UserId)
                {
                    throw new InvalidOperationException("Email is already in use by another user.");
                }
                user.ChangeEmail(request.Email);
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                user.ChangeRole(request.Role);
            }
        }

        // Password update (allowed for self or admin updating others)
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.SetPassword(passwordHash);
        }

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}

