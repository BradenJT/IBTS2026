using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Features.Auth.RegisterUser;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;
using Moq;

namespace IBTS2026.Tests.Application.Handlers;

[TestClass]
public sealed class RegisterUserHandlerTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IUserInvitationRepository> _invitationRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IPasswordHashingService> _passwordHasherMock = null!;
    private Mock<IValidator<RegisterUserCommand>> _validatorMock = null!;
    private RegisterUserHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _invitationRepositoryMock = new Mock<IUserInvitationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHashingService>();
        _validatorMock = new Mock<IValidator<RegisterUserCommand>>();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        _handler = new RegisterUserHandler(
            _userRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _validatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_FirstUser_ShouldCreateAdminWithoutToken()
    {
        // Arrange
        var command = new RegisterUserCommand("admin@test.com", "Password123!", "Admin", "User");

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Role.Should().Be("Admin");
        result.Email.Should().Be("admin@test.com");

        _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u => u.Role == "Admin")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_SubsequentUser_WithoutToken_ShouldFail()
    {
        // Arrange
        var command = new RegisterUserCommand("user@test.com", "Password123!", "Regular", "User");

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invite-only");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_SubsequentUser_WithValidToken_ShouldCreateUserWithInvitedRole()
    {
        // Arrange
        var invitation = UserInvitation.Create("user@test.com", "User", 1);
        var command = new RegisterUserCommand("user@test.com", "Password123!", "Regular", "User", invitation.Token);

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Role.Should().Be("User");
        result.Email.Should().Be("user@test.com");

        _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u => u.Role == "User")), Times.Once);
        _invitationRepositoryMock.Verify(r => r.Update(It.Is<UserInvitation>(i => i.IsUsed)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        // Arrange
        var command = new RegisterUserCommand("user@test.com", "Password123!", "Regular", "User", "invalid_token");

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync("invalid_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid invitation token");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithExpiredToken_ShouldFail()
    {
        // Arrange
        var invitation = UserInvitation.Create("user@test.com", "User", 1, expirationDays: -1); // Already expired
        var command = new RegisterUserCommand("user@test.com", "Password123!", "Regular", "User", invitation.Token);

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithMismatchedEmail_ShouldFail()
    {
        // Arrange
        var invitation = UserInvitation.Create("invited@test.com", "User", 1);
        var command = new RegisterUserCommand("different@test.com", "Password123!", "Regular", "User", invitation.Token);

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not match");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithExistingEmail_ShouldFail()
    {
        // Arrange
        var existingUser = User.Create("existing@test.com", "Existing", "User", "User");
        var invitation = UserInvitation.Create("existing@test.com", "User", 1);
        var command = new RegisterUserCommand("existing@test.com", "Password123!", "New", "User", invitation.Token);

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already registered");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithUsedToken_ShouldFail()
    {
        // Arrange
        var invitation = UserInvitation.Create("user@test.com", "User", 1);
        invitation.MarkAsUsed();
        var command = new RegisterUserCommand("user@test.com", "Password123!", "Regular", "User", invitation.Token);

        _userRepositoryMock.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired or has already been used");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }
}
