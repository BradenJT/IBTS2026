using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Entities.Features.Incidents.Incident;
using IBTS2026.Domain.Entities.Features.Incidents.IncidentNote;
using IBTS2026.Domain.Entities.Features.Notifications.NotificationOutbox;
using IBTS2026.Domain.Entities.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Persistence;

public partial class IBTS2026Context
{
    public virtual DbSet<NotificationOutbox> NotificationOutbox { get; set; } = null!;
    public virtual DbSet<IncidentNote> IncidentNotes { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        SeedPriorities(modelBuilder);
        SeedStatuses(modelBuilder);
        ConfigureNotificationOutbox(modelBuilder);
        ConfigureIncidentNotes(modelBuilder);
        ConfigureIncidentUserRelationships(modelBuilder);
        ConfigureUserAuthenticationFields(modelBuilder);
    }

    private static void ConfigureUserAuthenticationFields(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.PasswordHash)
                .IsUnicode(false);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.FailedLoginCount)
                .HasDefaultValue(0);
        });
    }

    private static void ConfigureIncidentUserRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasOne(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Incident_CreatedByUser");

            entity.HasOne(i => i.AssignedToUser)
                .WithMany()
                .HasForeignKey(i => i.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Incident_AssignedToUser");
        });
    }

    private static void ConfigureNotificationOutbox(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationOutbox>(entity =>
        {
            entity.HasKey(e => e.NotificationOutboxId);

            entity.ToTable("NotificationOutbox");

            entity.HasIndex(e => e.ProcessedAt, "IX_NotificationOutbox_ProcessedAt");
            entity.HasIndex(e => new { e.ProcessedAt, e.FailedAt, e.RetryCount }, "IX_NotificationOutbox_PendingNotifications");

            entity.Property(e => e.NotificationType)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.RecipientEmail)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.Property(e => e.Body)
                .IsRequired()
                .IsUnicode(false);
        });
    }

    private static void ConfigureIncidentNotes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncidentNote>(entity =>
        {
            entity.HasKey(e => e.IncidentNoteId);

            entity.ToTable("IncidentNote");

            entity.HasIndex(e => e.IncidentId, "IX_IncidentNote_IncidentId");
            entity.HasIndex(e => e.CreatedAt, "IX_IncidentNote_CreatedAt");

            entity.Property(e => e.Content)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.Incident)
                .WithMany()
                .HasForeignKey(d => d.IncidentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_IncidentNote_Incident");

            entity.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentNote_User");
        });
    }

    private static void SeedPriorities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Priority>().HasData(
            new Priority { PriorityId = 1, PriorityName = "Low" },
            new Priority { PriorityId = 2, PriorityName = "Medium" },
            new Priority { PriorityId = 3, PriorityName = "High" },
            new Priority { PriorityId = 4, PriorityName = "Critical" }
        );
    }

    private static void SeedStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>().HasData(
            new Status { StatusId = 1, StatusName = "Open" },
            new Status { StatusId = 2, StatusName = "In Progress" },
            new Status { StatusId = 3, StatusName = "Closed" },
            new Status { StatusId = 4, StatusName = "Unknown" }
        );
    }
}
