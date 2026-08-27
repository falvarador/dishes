using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dishes.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipePublicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
                columns: new[] { "CreatedAt", "PublishedAt", "Status", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 15, 12, 0, 0, 0, DateTimeKind.Utc), null, 0, new DateTime(2024, 11, 15, 12, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("bf2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
                columns: new[] { "CreatedAt", "PublishedAt", "Status", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 25, 12, 0, 0, 0, DateTimeKind.Utc), null, 0, new DateTime(2024, 11, 25, 12, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Recipes");

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 28, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1065), new DateTime(2026, 7, 28, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1557) });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("bf2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 7, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1932), new DateTime(2026, 8, 7, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1933) });
        }
    }
}
