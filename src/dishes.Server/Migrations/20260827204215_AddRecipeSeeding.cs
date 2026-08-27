using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace dishes.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RecipeTags",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("ab0a8f33-acc8-47f6-fbe1-ca4b1b6a2f3a"), "Vegan" },
                    { new Guid("bc1b9a44-bdd9-48a7-acb2-db5c2c7b3a4b"), "Gluten-Free" },
                    { new Guid("cd2c0b55-ceea-49b8-bdc3-ec6d3d8c4b5c"), "Quick" },
                    { new Guid("de3d1c66-dffb-40c9-ceb4-fd7e4e9d5c6d"), "Comfort Food" },
                    { new Guid("fa9f7e22-fbb7-46e5-ead0-bf3a0a5f1e2f"), "Vegetarian" }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CoverPhotoPath", "CreatedAt", "CreatorId", "Description", "Difficulty", "PrepTime", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "/images/carbonara.jpg", new DateTime(2026, 7, 28, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1065), new Guid("aa1a2b3c-4d5e-6f7a-8b9c-0d1e2f3a4b5c"), "Classic Italian pasta with eggs, cheese, and bacon", "Easy", "30 mins", "Spaghetti Carbonara", new DateTime(2026, 7, 28, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1557) },
                    { new Guid("bf2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"), "/images/beefstew.jpg", new DateTime(2026, 8, 7, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1932), new Guid("aa1a2b3c-4d5e-6f7a-8b9c-0d1e2f3a4b5c"), "Hearty beef stew with vegetables and rich gravy", "Medium", "2 hours", "Beef Stew", new DateTime(2026, 8, 7, 20, 42, 14, 969, DateTimeKind.Utc).AddTicks(1933) }
                });

            migrationBuilder.InsertData(
                table: "RecipeIngredients",
                columns: new[] { "Id", "IngredientName", "Quantity", "RecipeId", "Unit" },
                values: new object[,]
                {
                    { new Guid("ca3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f"), "Spaghetti", 400m, new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "g" },
                    { new Guid("db4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a"), "Eggs", 4m, new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "whole" },
                    { new Guid("ec5f6a7b-8c9d-0e1f-2a3b-4c5d6e7f8a9b"), "Parmesan Cheese", 200m, new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "g" },
                    { new Guid("fd6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c"), "Bacon", 150m, new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "g" }
                });

            migrationBuilder.InsertData(
                table: "RecipeInstructions",
                columns: new[] { "Id", "Description", "RecipeId", "StepNumber" },
                values: new object[,]
                {
                    { new Guid("ae7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c1d"), "Cook spaghetti in salted boiling water until al dente", new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 1 },
                    { new Guid("bf8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e"), "Cook bacon until crispy and chop into pieces", new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 2 },
                    { new Guid("ca9d0e1f-2a3b-4c5d-6e7f-8a9b0c1d2e3f"), "Mix eggs and grated cheese in a bowl", new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 3 },
                    { new Guid("db0e1f2a-3b4c-5d6e-7f8a-9b0c1d2e3f4a"), "Drain pasta and mix with bacon, then toss with egg mixture off heat", new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: new Guid("ca3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f"));

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: new Guid("db4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a"));

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: new Guid("ec5f6a7b-8c9d-0e1f-2a3b-4c5d6e7f8a9b"));

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: new Guid("fd6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c"));

            migrationBuilder.DeleteData(
                table: "RecipeInstructions",
                keyColumn: "Id",
                keyValue: new Guid("ae7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c1d"));

            migrationBuilder.DeleteData(
                table: "RecipeInstructions",
                keyColumn: "Id",
                keyValue: new Guid("bf8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e"));

            migrationBuilder.DeleteData(
                table: "RecipeInstructions",
                keyColumn: "Id",
                keyValue: new Guid("ca9d0e1f-2a3b-4c5d-6e7f-8a9b0c1d2e3f"));

            migrationBuilder.DeleteData(
                table: "RecipeInstructions",
                keyColumn: "Id",
                keyValue: new Guid("db0e1f2a-3b4c-5d6e-7f8a-9b0c1d2e3f4a"));

            migrationBuilder.DeleteData(
                table: "RecipeTags",
                keyColumn: "Id",
                keyValue: new Guid("ab0a8f33-acc8-47f6-fbe1-ca4b1b6a2f3a"));

            migrationBuilder.DeleteData(
                table: "RecipeTags",
                keyColumn: "Id",
                keyValue: new Guid("bc1b9a44-bdd9-48a7-acb2-db5c2c7b3a4b"));

            migrationBuilder.DeleteData(
                table: "RecipeTags",
                keyColumn: "Id",
                keyValue: new Guid("cd2c0b55-ceea-49b8-bdc3-ec6d3d8c4b5c"));

            migrationBuilder.DeleteData(
                table: "RecipeTags",
                keyColumn: "Id",
                keyValue: new Guid("de3d1c66-dffb-40c9-ceb4-fd7e4e9d5c6d"));

            migrationBuilder.DeleteData(
                table: "RecipeTags",
                keyColumn: "Id",
                keyValue: new Guid("fa9f7e22-fbb7-46e5-ead0-bf3a0a5f1e2f"));

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("bf2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"));

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: new Guid("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"));
        }
    }
}
