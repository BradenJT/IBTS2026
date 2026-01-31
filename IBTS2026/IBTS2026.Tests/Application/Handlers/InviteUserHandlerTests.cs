using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Features.Auth.InviteUser;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;
using Moq;

namespace IBTS2026.Tests.Application.Handlers;

[TestClass]
public sealed class InviteUserHandlerTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IUserInvitationRepository> _invitationRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<IValidator<InviteUserCommand>> _validatorMock = null!;
    private InviteUserHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _invitationRepositoryMock = new Mock<IUserInvitationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<INotificationService>();
        _validatorMock = new Mock<IValidator<InviteUserCommand>>();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<InviteUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new InviteUserHandler(
            _userRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _validatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_AdminInvitesUser_ShouldSucceed()
    {
        // Arrange
        var adminUser = User.Create("admin@test.com", "Admin", "User", "Admin");
        adminUser.GetType().GetProperty("UserId")!.SetValue(adminUser, 1);

        var command = new InviteUserCommand("newuser@test.com", "User", 1);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("newuser@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _invitationRepositoryMock.Setup(r => r.GetByEmailAsync("newuser@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Email.Should().Be("newuser@test.com");
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _invitationRepositoryMock.Verify(r => r.Add(It.IsAny<UserInvitation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.QueueInvitationNotification(
            "newuser@test.com", It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_NonAdminInvitesUser_ShouldFail()
    {
        // Arrange
        var regularUser = User.Create("user@test.com", "Regular", "User", "User");
        regularUser.GetType().GetProperty("UserId")!.SetValue(regularUser, 2);

        var command = new InviteUserCommand("newuser@test.com", "User", 2);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regularUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only administrators");
        _invitationRepositoryMock.Verify(r => r.Add(It.IsAny<UserInvitation>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_InviteExistingUser_ShouldFail()
    {
        // Arrange
        var adminUser = User.Create("admin@test.com", "Admin", "User", "Admin");
        adminUser.GetType().GetProperty("UserId")!.SetValue(adminUser, 1);

        var existingUser = User.Create("existing@test.com", "Existing", "User", "User");

        var command = new InviteUserCommand("existing@test.com", "User", 1);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
        _invitationRepositoryMock.Verify(r => r.Add(It.IsAny<UserInvitation>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_PendingInvitationExists_ShouldFail()
    {
        // Arrange
        var adminUser = User.Create("admin@test.com", "Admin", "User", "Admin");
        adminUser.GetType().GetProperty("UserId")!.SetValue(adminUser, 1);

        var existingInvitation = UserInvitation.Create("pending@test.com", "User", 1);

        var command = new InviteUserCommand("pending@test.com", "User", 1);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("pending@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _invitationRepositoryMock.Setup(r => r.GetByEmailAsync("pending@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("active invitation already exists");
        _invitationRepositoryMock.Verify(r => r.Add(It.IsAny<UserInvitation>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_InvitingUserNotFound_ShouldFail()
    {
        // Arrange
        var command = new InviteUserCommand("newuser@test.com", "User", 999);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        _invitationRepositoryMock.Verify(r => r.Add(It.IsAny<UserInvitation>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_AdminInvitesAdmin_ShouldSucceed()
    {
        // Arrange
        var adminUser = User.Create("admin@test.com", "Admin", "User", "Admin");
        adminUser.GetType().GetProperty("UserId")!.SetValue(adminUser, 1);

        var command = new InviteUserCommand("newadmin@test.com", "Admin", 1);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("newadmin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _invitationRepositoryMock.Setup(r => r.GetByEmailAsync("newadmin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _invitationRepositoryMock.Verify(r => r.Add(It.Is<UserInvitation>(i => i.Role == "Admin")), Times.Once);
    }
}
