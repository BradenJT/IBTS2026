using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Users.UpdateUser
{
    internal class UpdateUserHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IValidator<UpdateUserCommand> validator) : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(_unitOfWork));
        private readonly IValidator<UpdateUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(request, ct);

            var user = await _users.GetByIdAsync(request.UserId, ct);

            if (user is null)
            {
                return false;
            }

            if (HasChanges(user, request))
            {
                User.Update(user, request.Email, request.FirstName, request.LastName, request.Role);
            }

            _users.Update(user);

            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }

        private static bool HasChanges(User user, UpdateUserCommand request) =>
            user.FirstName != request.FirstName ||
            user.LastName != request.LastName ||
            user.Role != request.Role ||
            user.Email != request.Email;
    }
}
