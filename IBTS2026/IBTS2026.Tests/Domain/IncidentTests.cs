using FluentAssertions;
using IBTS2026.Domain.Entities.Features.Incidents.Incident;
using IBTS2026.Domain.Enums;

namespace IBTS2026.Tests.Domain;

[TestClass]
public sealed class IncidentTests
{
    [TestMethod]
    public void Create_WithValidData_ShouldCreateIncident()
    {
        // Arrange
        var title = "Test Incident";
        var description = "Test Description";
        var statusId = (int)IncidentStatus.Open;
        var priorityId = 1;
        var createdByUserId = 1;
        int? assignedToUserId = 2;

        // Act
        var incident = Incident.Create(title, description, statusId, priorityId, createdByUserId, assignedToUserId);

        // Assert
        incident.Title.Should().Be(title);
        incident.Description.Should().Be(description);
        incident.StatusId.Should().Be(statusId);
        incident.PriorityId.Should().Be(priorityId);
        incident.CreatedBy.Should().Be(createdByUserId);
        incident.AssignedTo.Should().Be(assignedToUserId);
        incident.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void ChangeTitle_ShouldUpdateTitle()
    {
        // Arrange
        var incident = CreateTestIncident();
        var newTitle = "Updated Title";

        // Act
        incident.ChangeTitle(newTitle);

        // Assert
        incident.Title.Should().Be(newTitle);
    }

    [TestMethod]
    public void ChangeDescription_ShouldUpdateDescription()
    {
        // Arrange
        var incident = CreateTestIncident();
        var newDescription = "Updated Description";

        // Act
        incident.ChangeDescription(newDescription);

        // Assert
        incident.Description.Should().Be(newDescription);
    }

    [TestMethod]
    public void ChangePriority_ShouldUpdatePriority()
    {
        // Arrange
        var incident = CreateTestIncident();
        var newPriorityId = 3;

        // Act
        incident.ChangePriority(newPriorityId);

        // Assert
        incident.PriorityId.Should().Be(newPriorityId);
    }

    [TestMethod]
    public void AssignTo_ShouldUpdateAssignment()
    {
        // Arrange
        var incident = CreateTestIncident();
        var newAssigneeId = 5;

        // Act
        incident.AssignTo(newAssigneeId);

        // Assert
        incident.AssignedTo.Should().Be(newAssigneeId);
    }

    [TestMethod]
    public void AssignTo_WithNull_ShouldClearAssignment()
    {
        // Arrange
        var incident = CreateTestIncident(assignedToUserId: 5);

        // Act
        incident.AssignTo(null);

        // Assert
        incident.AssignedTo.Should().BeNull();
    }

    [TestMethod]
    [DataRow(IncidentStatus.Open, IncidentStatus.InProgress)]
    [DataRow(IncidentStatus.Open, IncidentStatus.Closed)]
    [DataRow(IncidentStatus.InProgress, IncidentStatus.Open)]
    [DataRow(IncidentStatus.InProgress, IncidentStatus.Closed)]
    [DataRow(IncidentStatus.Closed, IncidentStatus.Open)]
    public void ChangeStatus_ValidTransition_ShouldUpdateStatus(IncidentStatus from, IncidentStatus to)
    {
        // Arrange
        var incident = CreateTestIncident(statusId: (int)from);

        // Act
        incident.ChangeStatus((int)to);

        // Assert
        incident.StatusId.Should().Be((int)to);
    }

    [TestMethod]
    [DataRow(IncidentStatus.Open, IncidentStatus.Open)]
    [DataRow(IncidentStatus.InProgress, IncidentStatus.InProgress)]
    [DataRow(IncidentStatus.Closed, IncidentStatus.Closed)]
    public void ChangeStatus_SameStatus_ShouldSucceed(IncidentStatus from, IncidentStatus to)
    {
        // Arrange
        var incident = CreateTestIncident(statusId: (int)from);

        // Act
        incident.ChangeStatus((int)to);

        // Assert
        incident.StatusId.Should().Be((int)to);
    }

    [TestMethod]
    [DataRow(IncidentStatus.Closed, IncidentStatus.InProgress)]
    public void ChangeStatus_InvalidTransition_ShouldThrow(IncidentStatus from, IncidentStatus to)
    {
        // Arrange
        var incident = CreateTestIncident(statusId: (int)from);

        // Act
        var action = () => incident.ChangeStatus((int)to);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"Invalid status transition from {from} to {to}.");
    }

    private static Incident CreateTestIncident(
        int statusId = (int)IncidentStatus.Open,
        int? assignedToUserId = null)
    {
        return Incident.Create(
            "Test Incident",
            "Test Description",
            statusId,
            1,
            1,
            assignedToUserId);
    }
}
