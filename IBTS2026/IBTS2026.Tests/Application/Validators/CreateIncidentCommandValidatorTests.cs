using FluentAssertions;
using FluentValidation.TestHelper;
using IBTS2026.Application.Features.Incidents.CreateIncident;

namespace IBTS2026.Tests.Application.Validators;

[TestClass]
public sealed class CreateIncidentCommandValidatorTests
{
    private readonly CreateIncidentCommandValidator _validator = new();

    [TestMethod]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new CreateIncidentCommand(
            "Test Incident",
            "Test Description",
            1,
            1,
            1,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_WithValidCommandAndAssignee_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new CreateIncidentCommand(
            "Test Incident",
            "Test Description",
            1,
            1,
            1,
            2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("", "Description", 1, 1, 1, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [TestMethod]
    public void Validate_WithTooLongTitle_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand(new string('a', 251), "Description", 1, 1, 1, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 250 characters.");
    }

    [TestMethod]
    public void Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "", 1, 1, 1, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required.");
    }

    [TestMethod]
    public void Validate_WithZeroStatusId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "Description", 0, 1, 1, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StatusId)
            .WithErrorMessage("StatusId must be greater than zero.");
    }

    [TestMethod]
    public void Validate_WithZeroPriorityId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "Description", 1, 0, 1, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriorityId)
            .WithErrorMessage("PriorityId must be greater than zero.");
    }

    [TestMethod]
    public void Validate_WithZeroCreatedByUserId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "Description", 1, 1, 0, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedByUserId)
            .WithErrorMessage("CreatedByUserId must be greater than zero.");
    }

    [TestMethod]
    public void Validate_WithZeroAssignedToUserId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "Description", 1, 1, 1, 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedToUserId)
            .WithErrorMessage("AssignedToUserId must be greater than zero when provided.");
    }

    [TestMethod]
    public void Validate_WithNegativeAssignedToUserId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateIncidentCommand("Title", "Description", 1, 1, 1, -1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedToUserId)
            .WithErrorMessage("AssignedToUserId must be greater than zero when provided.");
    }
}
