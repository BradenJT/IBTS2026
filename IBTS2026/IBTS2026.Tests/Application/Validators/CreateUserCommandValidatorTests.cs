using FluentAssertions;
using FluentValidation.TestHelper;
using IBTS2026.Application.Features.Users.CreateUser;

namespace IBTS2026.Tests.Application.Validators;

[TestClass]
public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [TestMethod]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "Doe", "Admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_WithEmptyFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "", "Doe", "Admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName cannot be empty.");
    }

    [TestMethod]
    public void Validate_WithTooLongFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", new string('a', 51), "Doe", "Admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName cannot exceed 50 characters.");
    }

    [TestMethod]
    public void Validate_WithEmptyLastName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "", "Admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("LastName cannot be empty.");
    }

    [TestMethod]
    public void Validate_WithTooLongLastName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", new string('a', 51), "Admin");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("LastName cannot exceed 50 characters.");
    }

    [TestMethod]
    public void Validate_WithEmptyRole_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "Doe", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role cannot be empty.");
    }

    [TestMethod]
    public void Validate_WithTooLongRole_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "Doe", new string('a', 21));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role cannot exceed 20 characters.");
    }
}
