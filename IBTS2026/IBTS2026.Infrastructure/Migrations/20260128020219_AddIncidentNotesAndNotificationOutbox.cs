using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBTS2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentNotesAndNotificationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentNote",
                columns: table => new
                {
                    IncidentNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentNote", x => x.IncidentNoteId);
                    table.ForeignKey(
                        name: "FK_IncidentNote_Incident",
                        column: x => x.IncidentId,
                        principalTable: "Incident",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentNote_User",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    NotificationOutboxId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RecipientEmail = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    Subject = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    RelatedIncidentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.NotificationOutboxId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incident_AssignedTo",
                table: "Incident",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_CreatedBy",
                table: "Incident",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentNote_CreatedAt",
                table: "IncidentNote",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentNote_CreatedByUserId",
                table: "IncidentNote",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentNote_IncidentId",
                table: "IncidentNote",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_PendingNotifications",
                table: "NotificationOutbox",
                columns: new[] { "ProcessedAt", "FailedAt", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_ProcessedAt",
                table: "NotificationOutbox",
                column: "ProcessedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Incident_AssignedToUser",
                table: "Incident",
                column: "AssignedTo",
                principalTable: "User",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incident_CreatedByUser",
                table: "Incident",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incident_AssignedToUser",
                table: "Incident");

            migrationBuilder.DropForeignKey(
                name: "FK_Incident_CreatedByUser",
                table: "Incident");

            migrationBuilder.DropTable(
                name: "IncidentNote");

            migrationBuilder.DropTable(
                name: "NotificationOutbox");

            migrationBuilder.DropIndex(
                name: "IX_Incident_AssignedTo",
                table: "Incident");

            migrationBuilder.DropIndex(
                name: "IX_Incident_CreatedBy",
                table: "Incident");
        }
    }
}
