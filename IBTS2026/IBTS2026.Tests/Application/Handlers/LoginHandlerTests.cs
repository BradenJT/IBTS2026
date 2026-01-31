using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Features.Auth.Login;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;
using Microsoft.Extensions.Logging;
using Moq;

namespace IBTS2026.Tests.Application.Handlers;

[TestClass]
public sealed class LoginHandlerTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IPasswordHashingService> _passwordHasherMock = null!;
    private Mock<ITokenService> _tokenServiceMock = null!;
    private Mock<IValidator<LoginCommand>> _validatorMock = null!;
    private Mock<ILogger<LoginHandler>> _loggerMock = null!;
    private LoginHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHashingService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _validatorMock = new Mock<IValidator<LoginCommand>>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test_jwt_token");

        _handler = new LoginHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.GetType().GetProperty("UserId")!.SetValue(user, 1);

        var command = new LoginCommand("test@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_password", "Password123!"))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().Be("test_jwt_token");
        result.Email.Should().Be("test@test.com");
        result.Role.Should().Be("User");

        _tokenServiceMock.Verify(t => t.GenerateToken(1, "test@test.com", "User", It.IsAny<string>()), Times.Once);
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_InvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [TestMethod]
    public async Task Handle_InvalidPassword_ShouldIncrementFailCount()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.GetType().GetProperty("UserId")!.SetValue(user, 1);

        var command = new LoginCommand("test@test.com", "WrongPassword!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_password", "WrongPassword!"))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
        user.FailedLoginCount.Should().Be(1);

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_InactiveAccount_ShouldFail()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.Deactivate();

        var command = new LoginCommand("test@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("disabled");
    }

    [TestMethod]
    public async Task Handle_LockedAccount_ShouldFail()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.Lock(TimeSpan.FromMinutes(15));

        var command = new LoginCommand("test@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");
    }

    [TestMethod]
    public async Task Handle_FiveFailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.GetType().GetProperty("UserId")!.SetValue(user, 1);
        // Set failed count to 4 so the 5th attempt triggers lockout
        for (int i = 0; i < 4; i++) user.RecordLoginFailure();

        var command = new LoginCommand("test@test.com", "WrongPassword!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_password", "WrongPassword!"))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked due to too many failed attempts");
        user.IsLockedOut.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_SuccessfulLogin_ShouldClearFailedCount()
    {
        // Arrange
        var user = User.CreateWithPassword("test@test.com", "Test", "User", "User", "hashed_password");
        user.GetType().GetProperty("UserId")!.SetValue(user, 1);
        // Set some failed attempts
        user.RecordLoginFailure();
        user.RecordLoginFailure();

        var command = new LoginCommand("test@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_password", "Password123!"))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        user.FailedLoginCount.Should().Be(0);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
