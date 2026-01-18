using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Users.UpdateUser
{
    internal class UpdateUserHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IValidator<UpdateUserCommand> validator) : IRequestHandler<UpdateUserCommand, bool>
    {
        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);

            var user = await users.GetByIdAsync(request.UserId, ct);

            if (user is null)
            {
                return false;
            }

            if (!string.Equals(user.FirstName, request.FirstName, StringComparison.Ordinal))
            {
                user.ChangeFirstName(request.FirstName);
            }

            if (!string.Equals(user.LastName, request.LastName, StringComparison.Ordinal))
            {
                user.ChangeLastName(request.LastName);
            }

            if (!string.Equals(user.Role, request.Role, StringComparison.Ordinal))
            {
                user.ChangeRole(request.Role);
            }

            users.Update(user);

            await unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
