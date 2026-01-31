using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBTS2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "User",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginCount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "User");
        }
    }
}
