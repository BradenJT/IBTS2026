using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Users.CreateUser
{
    public sealed class CreateUserHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IValidator<CreateUserCommand> validator) : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IValidator<CreateUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<int> Handle(
            CreateUserCommand command,
            CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var user = User.Create(
                command.Email,
                command.FirstName,
                command.LastName,
                command.Role);

            _users.Add(user);

            await _unitOfWork.SaveChangesAsync(ct);

            return user.UserId;
        }
    }
}
