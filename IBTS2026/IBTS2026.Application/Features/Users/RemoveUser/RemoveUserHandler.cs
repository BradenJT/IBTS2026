using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Users.RemoveUser
{
    public sealed class RemoveUserHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IValidator<RemoveUserCommand> validator) : IRequestHandler<RemoveUserCommand, bool>
    {
        private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IValidator<RemoveUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<bool> Handle(RemoveUserCommand request, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(request, ct);

            var user = await _users.GetByIdAsync(request.UserId, ct);

            if (user is null)
            {
                return false;
            }

            _users.Remove(user);

            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
