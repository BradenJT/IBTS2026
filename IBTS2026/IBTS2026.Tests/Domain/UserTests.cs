using FluentAssertions;
using IBTS2026.Domain.Entities.Features.Users;

namespace IBTS2026.Tests.Domain;

[TestClass]
public sealed class UserTests
{
    [TestMethod]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var role = "Admin";

        // Act
        var user = User.Create(email, firstName, lastName, role);

        // Assert
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Role.Should().Be(role);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void ChangeFirstName_ShouldUpdateFirstName()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "Admin");
        var newFirstName = "Jane";

        // Act
        user.ChangeFirstName(newFirstName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
    }

    [TestMethod]
    public void ChangeLastName_ShouldUpdateLastName()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "Admin");
        var newLastName = "Smith";

        // Act
        user.ChangeLastName(newLastName);

        // Assert
        user.LastName.Should().Be(newLastName);
    }

    [TestMethod]
    public void ChangeEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "Admin");
        var newEmail = "newemail@example.com";

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
    }

    [TestMethod]
    public void ChangeRole_ShouldUpdateRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "Admin");
        var newRole = "User";

        // Act
        user.ChangeRole(newRole);

        // Assert
        user.Role.Should().Be(newRole);
    }
}
