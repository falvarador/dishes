using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dishes.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingsAndFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRecipeFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecipeFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecipeFavorites_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRecipeRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecipeRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecipeRatings_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRecipeFavorites_RecipeId",
                table: "UserRecipeFavorites",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecipeFavorites_UserId_RecipeId",
                table: "UserRecipeFavorites",
                columns: new[] { "UserId", "RecipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRecipeRatings_RecipeId",
                table: "UserRecipeRatings",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecipeRatings_UserId_RecipeId",
                table: "UserRecipeRatings",
                columns: new[] { "UserId", "RecipeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecipeFavorites");

            migrationBuilder.DropTable(
                name: "UserRecipeRatings");
        }
    }
}
