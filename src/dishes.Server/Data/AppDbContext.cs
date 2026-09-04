using dishes.Server.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppIdentityUser>(options)
{
    public DbSet<Dish> Dishes { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<RecipeInstruction> RecipeInstructions { get; set; } = null!;
    public DbSet<RecipeCategory> RecipeCategories { get; set; } = null!;
    public DbSet<RecipeTag> RecipeTags { get; set; } = null!;
    public DbSet<UserRecipeRating> UserRecipeRatings { get; set; } = null!;
    public DbSet<UserRecipeFavorite> UserRecipeFavorites { get; set; } = null!;
    public DbSet<Badge> Badges { get; set; } = null!;
    public DbSet<UserBadge> UserBadges { get; set; } = null!;
    public DbSet<UserFollow> UserFollows { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Ingredient>().HasData(
            new(Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Beef"),
            new(Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"), "Onion"),
            new(Guid.Parse("c19099ed-94db-44ba-885b-0ad7205d5e40"), "Dark beer"),
            new(Guid.Parse("0c4dc798-b38b-4a1c-905c-a9e76dbef17b"), "Brown piece of bread"),
            new(Guid.Parse("937b1ba1-7969-4324-9ab5-afb0e4d875e6"), "Mustard"),
            new(Guid.Parse("7a2fbc72-bb33-49de-bd23-c78fceb367fc"), "Chicory"),
            new(Guid.Parse("b5f336e2-c226-4389-aac3-2499325a3de9"), "Mayo"),
            new(Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe"), "Various spices"),
            new(Guid.Parse("aab31c70-57ce-4b6d-a66c-9c1b094e915d"), "Mussels"),
            new(Guid.Parse("fef8b722-664d-403f-ae3c-05f8ed7d7a1f"), "Celery"),
            new(Guid.Parse("8d5a1b40-6677-4545-b6e8-5ba93efda0a1"), "French fries"),
            new(Guid.Parse("40563e5b-e538-4084-9587-3df74fae21d4"), "Tomato"),
            new(Guid.Parse("f350e1a0-38de-42fe-ada5-ae436378ee5b"), "Tomato paste"),
            new(Guid.Parse("d5cad9a4-144e-4a3d-858d-9840792fa65d"), "Bay leave"),
            new(Guid.Parse("b617df23-3d91-40e1-99aa-b07d264aa937"), "Carrot"),
            new(Guid.Parse("b8b9a6ae-9bcc-4fb3-b883-5974e04eda56"), "Garlic"),
            new(Guid.Parse("ecd396c3-4403-4fbf-83ca-94a8e9d859b3"), "Red wine"),
            new(Guid.Parse("c2c75b40-2453-416e-a7ed-3505b121d671"), "Coconut milk"),
            new(Guid.Parse("3bd3f0a1-87d3-4b85-94fa-ba92bd1874e7"), "Ginger"),
            new(Guid.Parse("047ab5cc-d041-486e-9d22-a0860fb13237"), "Chili pepper"),
            new(Guid.Parse("e0017fe1-773f-4a59-9730-9489833c6e8e"), "Tamarind paste"),
            new(Guid.Parse("c9b46f9c-d6ce-42c3-8736-2cddbbadee10"), "Firm fish"),
            new(Guid.Parse("a07cde83-3127-45da-bbd5-04a7c8d13aa4"), "Ginger garlic paste"),
            new(Guid.Parse("ebe94d5d-2ad8-4886-b246-05a1fad83d1c"), "Garam masala"));

        _ = modelBuilder.Entity<Dish>().HasData(
           new(Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"),
            "Flemish Beef stew with chicory"),
           new(Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"),
            "Mussels with french fries"),
           new(Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"),
           "Ragu alla bolognaise"),
           new(Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"),
           "Rendang"),
           new(Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"),
           "Fish Masala"));

        _ = modelBuilder
            .Entity<Dish>()
            .HasMany(d => d.Ingredients)
            .WithMany(i => i.Dishes)
            .UsingEntity(e => e.HasData(
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("c19099ed-94db-44ba-885b-0ad7205d5e40") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("0c4dc798-b38b-4a1c-905c-a9e76dbef17b") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("937b1ba1-7969-4324-9ab5-afb0e4d875e6") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("7a2fbc72-bb33-49de-bd23-c78fceb367fc") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("b5f336e2-c226-4389-aac3-2499325a3de9") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe") },
                new { DishesId = Guid.Parse("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06"), IngredientsId = Guid.Parse("d5cad9a4-144e-4a3d-858d-9840792fa65d") },
                new { DishesId = Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"), IngredientsId = Guid.Parse("aab31c70-57ce-4b6d-a66c-9c1b094e915d") },
                new { DishesId = Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"), IngredientsId = Guid.Parse("fef8b722-664d-403f-ae3c-05f8ed7d7a1f") },
                new { DishesId = Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"), IngredientsId = Guid.Parse("8d5a1b40-6677-4545-b6e8-5ba93efda0a1") },
                new { DishesId = Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"), IngredientsId = Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe") },
                new { DishesId = Guid.Parse("fe462ec7-b30c-4987-8a8e-5f7dbd8e0cfa"), IngredientsId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("40563e5b-e538-4084-9587-3df74fae21d4") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("f350e1a0-38de-42fe-ada5-ae436378ee5b") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("d5cad9a4-144e-4a3d-858d-9840792fa65d") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("fef8b722-664d-403f-ae3c-05f8ed7d7a1f") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("b617df23-3d91-40e1-99aa-b07d264aa937") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("b8b9a6ae-9bcc-4fb3-b883-5974e04eda56") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("ecd396c3-4403-4fbf-83ca-94a8e9d859b3") },
                new { DishesId = Guid.Parse("b512d7cf-b331-4b54-8dae-d1228d128e8d"), IngredientsId = Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("c2c75b40-2453-416e-a7ed-3505b121d671") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("b8b9a6ae-9bcc-4fb3-b883-5974e04eda56") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("3bd3f0a1-87d3-4b85-94fa-ba92bd1874e7") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("047ab5cc-d041-486e-9d22-a0860fb13237") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("e0017fe1-773f-4a59-9730-9489833c6e8e") },
                new { DishesId = Guid.Parse("fd630a57-2352-4731-b25c-db9cc7601b16"), IngredientsId = Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("c9b46f9c-d6ce-42c3-8736-2cddbbadee10") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("a07cde83-3127-45da-bbd5-04a7c8d13aa4") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("ebe94d5d-2ad8-4886-b246-05a1fad83d1c") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("40563e5b-e538-4084-9587-3df74fae21d4") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("c2c75b40-2453-416e-a7ed-3505b121d671") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("d5cad9a4-144e-4a3d-858d-9840792fa65d") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("047ab5cc-d041-486e-9d22-a0860fb13237") },
                new { DishesId = Guid.Parse("98929bd4-f099-41eb-a994-f1918b724b5a"), IngredientsId = Guid.Parse("c22bec27-a880-4f2a-b380-12dcd99c61fe") }
                ));

        // Configure Recipe relationships
        _ = modelBuilder.Entity<RecipeRecipeCategory>()
            .HasKey(rc => new { rc.RecipeId, rc.CategoryId });

        _ = modelBuilder.Entity<RecipeRecipeCategory>()
            .HasOne(rc => rc.Recipe)
            .WithMany(r => r.Categories)
            .HasForeignKey(rc => rc.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<RecipeRecipeCategory>()
            .HasOne(rc => rc.Category)
            .WithMany(c => c.Recipes)
            .HasForeignKey(rc => rc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<RecipeRecipeTag>()
            .HasKey(rt => new { rt.RecipeId, rt.TagId });

        _ = modelBuilder.Entity<RecipeRecipeTag>()
            .HasOne(rt => rt.Recipe)
            .WithMany(r => r.Tags)
            .HasForeignKey(rt => rt.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<RecipeRecipeTag>()
            .HasOne(rt => rt.Tag)
            .WithMany(t => t.Recipes)
            .HasForeignKey(rt => rt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<RecipeInstruction>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.Instructions)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed recipe categories
        _ = modelBuilder.Entity<RecipeCategory>().HasData(
            new(Guid.Parse("6b0d8b33-cdf8-47d6-b3a1-8d5f1e9a2c3b"), "Main Course"),
            new(Guid.Parse("7c1e9c44-def9-48e7-c4b2-9e6f2f0b3d4c"), "Appetizer"),
            new(Guid.Parse("8d2f0d55-efa0-49f8-d5c3-af7b3a1c4e5d"), "Dessert"),
            new(Guid.Parse("9e3a1e66-fab1-40a9-e6d4-ba8b4b2d5f6e"), "Breakfast"),
            new(Guid.Parse("af4a2f77-abc2-41b0-f7e5-cb9c5c3e6a7f"), "Seafood"),
            new(Guid.Parse("ba5b3a88-bcd3-42c1-a8f6-dc0d6c4f7b8a"), "Baking"),
            new(Guid.Parse("cb6c4b99-cde4-43d2-b9a7-ed1e7d5a8c9b"), "Lunch"),
            new(Guid.Parse("dc7d5c00-def5-44e3-caa8-fe2f8e6b9d0c"), "Dinner"),
            new(Guid.Parse("ed8e6d11-efa6-45f4-dbc9-af3a9f7c0e1d"), "Snack"));

        // Seed recipe tags
        _ = modelBuilder.Entity<RecipeTag>().HasData(
            new(Guid.Parse("fa9f7e22-fbb7-46e5-ead0-bf3a0a5f1e2f"), "Vegetarian"),
            new(Guid.Parse("ab0a8f33-acc8-47f6-fbe1-ca4b1b6a2f3a"), "Vegan"),
            new(Guid.Parse("bc1b9a44-bdd9-48a7-acb2-db5c2c7b3a4b"), "Gluten-Free"),
            new(Guid.Parse("cd2c0b55-ceea-49b8-bdc3-ec6d3d8c4b5c"), "Quick"),
            new(Guid.Parse("de3d1c66-dffb-40c9-ceb4-fd7e4e9d5c6d"), "Comfort Food"));

        // Seed sample recipes
        _ = modelBuilder.Entity<Recipe>().HasData(
            new(
                Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
                "Spaghetti Carbonara",
                "Classic Italian pasta with eggs, cheese, and bacon",
                "30 mins",
                "Easy",
                Guid.Parse("aa1a2b3c-4d5e-6f7a-8b9c-0d1e2f3a4b5c")
            )
            {
                CoverPhotoPath = "/images/carbonara.jpg",
                CreatedAt = new DateTime(2024, 11, 15, 12, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 11, 15, 12, 0, 0, DateTimeKind.Utc)
            },
            new(
                Guid.Parse("bf2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
                "Beef Stew",
                "Hearty beef stew with vegetables and rich gravy",
                "2 hours",
                "Medium",
                Guid.Parse("aa1a2b3c-4d5e-6f7a-8b9c-0d1e2f3a4b5c")
            )
            {
                CoverPhotoPath = "/images/beefstew.jpg",
                CreatedAt = new DateTime(2024, 11, 25, 12, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 11, 25, 12, 0, 0, DateTimeKind.Utc)
            });

        // Seed ingredients for Carbonara
        _ = modelBuilder.Entity<RecipeIngredient>().HasData(
            new(Guid.Parse("ca3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "Spaghetti", 400m, "g"),
            new(Guid.Parse("db4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "Eggs", 4m, "whole"),
            new(Guid.Parse("ec5f6a7b-8c9d-0e1f-2a3b-4c5d6e7f8a9b"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "Parmesan Cheese", 200m, "g"),
            new(Guid.Parse("fd6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), "Bacon", 150m, "g"));

        // Seed instructions for Carbonara
        _ = modelBuilder.Entity<RecipeInstruction>().HasData(
            new(Guid.Parse("ae7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c1d"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 1, "Cook spaghetti in salted boiling water until al dente"),
            new(Guid.Parse("bf8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 2, "Cook bacon until crispy and chop into pieces"),
            new(Guid.Parse("ca9d0e1f-2a3b-4c5d-6e7f-8a9b0c1d2e3f"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 3, "Mix eggs and grated cheese in a bowl"),
            new(Guid.Parse("db0e1f2a-3b4c-5d6e-7f8a-9b0c1d2e3f4a"), Guid.Parse("af1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), 4, "Drain pasta and mix with bacon, then toss with egg mixture off heat"));

        // Configure UserRecipeRating relationship
        _ = modelBuilder
            .Entity<UserRecipeRating>()
            .HasOne(r => r.Recipe)
            .WithMany(r => r.Ratings)
            .HasForeignKey(r => r.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserRecipeFavorite relationship
        _ = modelBuilder
            .Entity<UserRecipeFavorite>()
            .HasOne(f => f.Recipe)
            .WithMany(r => r.Favorites)
            .HasForeignKey(f => f.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add unique constraints for rating (one rating per user per recipe)
        _ = modelBuilder
            .Entity<UserRecipeRating>()
            .HasIndex(r => new { r.UserId, r.RecipeId })
            .IsUnique();

        // Add unique constraints for favorite (user cannot favorite recipe twice)
        _ = modelBuilder
            .Entity<UserRecipeFavorite>()
            .HasIndex(f => new { f.UserId, f.RecipeId })
            .IsUnique();

        // Configure Badge relationships
        _ = modelBuilder.Entity<UserBadge>()
            .HasOne(ub => ub.User)
            .WithMany()
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<UserBadge>()
            .HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add unique constraint for user badges (user cannot have same badge twice)
        _ = modelBuilder
            .Entity<UserBadge>()
            .HasIndex(ub => new { ub.UserId, ub.BadgeId })
            .IsUnique();

        _ = modelBuilder.Entity<UserFollow>()
            .HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        _ = modelBuilder.Entity<UserFollow>()
            .HasOne(f => f.Followed)
            .WithMany()
            .HasForeignKey(f => f.FollowedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        _ = modelBuilder
            .Entity<UserFollow>()
            .HasIndex(f => new { f.FollowerUserId, f.FollowedUserId })
            .IsUnique();

        _ = modelBuilder
            .Entity<UserFollow>()
            .HasIndex(f => f.FollowedUserId);

        base.OnModelCreating(modelBuilder);
    }
}
