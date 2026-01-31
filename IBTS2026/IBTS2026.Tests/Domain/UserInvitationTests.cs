using FluentAssertions;
using IBTS2026.Domain.Entities.Features.Users;

namespace IBTS2026.Tests.Domain;

[TestClass]
public sealed class UserInvitationTests
{
    [TestMethod]
    public void Create_WithValidData_ShouldCreateInvitation()
    {
        // Arrange
        var email = "test@example.com";
        var role = "User";
        var invitedByUserId = 1;

        // Act
        var invitation = UserInvitation.Create(email, role, invitedByUserId);

        // Assert
        invitation.Email.Should().Be(email);
        invitation.Role.Should().Be(role);
        invitation.InvitedByUserId.Should().Be(invitedByUserId);
        invitation.Token.Should().NotBeNullOrEmpty();
        invitation.Token.Should().HaveLength(32); // Guid without hyphens
        invitation.IsUsed.Should().BeFalse();
        invitation.UsedAt.Should().BeNull();
        invitation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void Create_WithCustomExpiration_ShouldSetCorrectExpiresAt()
    {
        // Arrange & Act
        var invitation = UserInvitation.Create("test@example.com", "User", 1, expirationDays: 14);

        // Assert
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void IsValid_NewInvitation_ShouldBeTrue()
    {
        // Arrange
        var invitation = UserInvitation.Create("test@example.com", "User", 1);

        // Act & Assert
        invitation.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void IsValid_UsedInvitation_ShouldBeFalse()
    {
        // Arrange
        var invitation = UserInvitation.Create("test@example.com", "User", 1);
        invitation.MarkAsUsed();

        // Act & Assert
        invitation.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public void IsValid_ExpiredInvitation_ShouldBeFalse()
    {
        // Arrange
        var invitation = UserInvitation.Create("test@example.com", "User", 1, expirationDays: -1);

        // Act & Assert
        invitation.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public void MarkAsUsed_ShouldSetIsUsedAndUsedAt()
    {
        // Arrange
        var invitation = UserInvitation.Create("test@example.com", "User", 1);

        // Act
        invitation.MarkAsUsed();

        // Assert
        invitation.IsUsed.Should().BeTrue();
        invitation.UsedAt.Should().NotBeNull();
        invitation.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void Token_ShouldBeUnique()
    {
        // Arrange & Act
        var invitation1 = UserInvitation.Create("test1@example.com", "User", 1);
        var invitation2 = UserInvitation.Create("test2@example.com", "User", 1);

        // Assert
        invitation1.Token.Should().NotBe(invitation2.Token);
    }

    [TestMethod]
    public void Create_AdminRole_ShouldSetAdminRole()
    {
        // Arrange & Act
        var invitation = UserInvitation.Create("admin@example.com", "Admin", 1);

        // Assert
        invitation.Role.Should().Be("Admin");
    }
}
