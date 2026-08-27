using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes;
using dishes.Server.Features.Recipes.UploadRecipeImage;
using dishes.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Recipes;

public class RecipeImageUploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecipeImageUploadTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task ClearRecipesAndImagesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var imageService = scope.ServiceProvider.GetRequiredService<IImageUploadService>();

        context.RecipeInstructions.RemoveRange(context.RecipeInstructions);
        context.RecipeIngredients.RemoveRange(context.RecipeIngredients);
        context.Recipes.RemoveRange(context.Recipes);
        context.RecipeCategories.RemoveRange(context.RecipeCategories);
        context.RecipeTags.RemoveRange(context.RecipeTags);
        await context.SaveChangesAsync();

        // Cleanup uploaded images
        await imageService.CleanupTestImagesAsync(CancellationToken.None);
    }

    private async Task<Recipe> SeedRecipeAsync(
        string title = "Test Recipe",
        string description = "A test recipe")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            "30 mins",
            "Easy",
            Guid.NewGuid()
        );
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();
        return recipe;
    }

    private byte[] CreateSimpleJpegImage()
    {
        // Minimal valid JPEG file (1x1 pixel, white)
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
            0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
            0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
            0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
            0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
            0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
            0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
            0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
            0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
            0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
            0x23, 0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24, 0x33, 0x62, 0x72,
            0x82, 0x09, 0x0A, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2A, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x43, 0x44, 0x45,
            0x46, 0x47, 0x48, 0x49, 0x4A, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5A, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x73, 0x74, 0x75,
            0x76, 0x77, 0x78, 0x79, 0x7A, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x8A, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0xA2, 0xA3,
            0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6,
            0xB7, 0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9,
            0xCA, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE1, 0xE2,
            0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xF1, 0xF2, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01,
            0x00, 0x00, 0x3F, 0x00, 0xFB, 0xD6, 0xFF, 0xD9
        };
    }

    [Fact]
    public async Task UploadRecipeImage_SuccessfullyUploads_WhenFileIsValid()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync("Image Test Recipe", "Test");

        var form = new MultipartFormDataContent();
        var imageData = CreateSimpleJpegImage();
        var fileContent = new ByteArrayContent(imageData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "Image", "test.jpg");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        Assert.NotNull(result);
        Assert.Equal(recipe.Id, result!.RecipeId);
        Assert.NotNull(result.ImagePath);
        Assert.StartsWith("/images/recipes/", result.ImagePath);

        // Verify recipe was updated in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);
        Assert.NotNull(dbRecipe);
        Assert.Equal(result.ImagePath, dbRecipe!.CoverPhotoPath);
    }

    [Fact]
    public async Task UploadRecipeImage_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var form = new MultipartFormDataContent();
        var imageData = CreateSimpleJpegImage();
        var fileContent = new ByteArrayContent(imageData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "Image", "test.jpg");

        var response = await _client.PostAsync($"/api/recipes/{Guid.NewGuid()}/image", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UploadRecipeImage_ReturnsBadRequest_WhenNoFileProvided()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        var form = new MultipartFormDataContent();

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadRecipeImage_ReturnsBadRequest_WhenFileIsEmpty()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[0]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "Image", "empty.jpg");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadRecipeImage_ReturnsBadRequest_WhenFileSizeExceeds1MB()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        // Create a file larger than 1MB
        var oversizedData = new byte[1024 * 1024 + 100]; // 1MB + 100 bytes
        Random.Shared.NextBytes(oversizedData);

        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(oversizedData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "Image", "large.jpg");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("error", result!);
        Assert.Contains("exceeds", result!["error"].ToString()!.ToLower());
    }

    [Fact]
    public async Task UploadRecipeImage_ReturnsBadRequest_WhenFileFormatIsInvalid()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02, 0x03 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "Image", "document.txt");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("error", result!);
    }

    [Fact]
    public async Task UploadRecipeImage_ReplacesOldImage_WhenRecipeHasExistingImage()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        // Upload first image
        var form1 = new MultipartFormDataContent();
        var imageData1 = CreateSimpleJpegImage();
        var fileContent1 = new ByteArrayContent(imageData1);
        fileContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form1.Add(fileContent1, "Image", "first.jpg");

        var response1 = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var result1 = await response1.Content.ReadFromJsonAsync<ImageUploadResponse>();
        var firstImagePath = result1!.ImagePath;

        // Upload second image
        var form2 = new MultipartFormDataContent();
        var imageData2 = CreateSimpleJpegImage();
        var fileContent2 = new ByteArrayContent(imageData2);
        fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form2.Add(fileContent2, "Image", "second.png");

        var response2 = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var result2 = await response2.Content.ReadFromJsonAsync<ImageUploadResponse>();
        var secondImagePath = result2!.ImagePath;

        // Verify paths are different
        Assert.NotEqual(firstImagePath, secondImagePath);

        // Verify recipe has the second image
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);
        Assert.NotNull(dbRecipe);
        Assert.Equal(secondImagePath, dbRecipe!.CoverPhotoPath);
    }

    [Fact]
    public async Task UploadRecipeImage_SuccessfullyUploads_PngFormat()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        var form = new MultipartFormDataContent();
        // Minimal PNG file (1x1 pixel)
        var pngData = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x44, 0x41,
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0x0F, 0x00, 0x00,
            0x01, 0x01, 0x00, 0x01, 0x13, 0xEE, 0x59, 0x07,
            0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44,
            0xAE, 0x42, 0x60, 0x82
        };
        var fileContent = new ByteArrayContent(pngData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "Image", "test.png");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        Assert.NotNull(result);
        Assert.EndsWith(".png", result!.ImagePath);
    }

    [Fact]
    public async Task UploadRecipeImage_SuccessfullyUploads_WebPFormat()
    {
        await ClearRecipesAndImagesAsync();
        var recipe = await SeedRecipeAsync();

        var form = new MultipartFormDataContent();
        // Minimal WebP file (1x1 pixel)
        var webpData = new byte[]
        {
            0x52, 0x49, 0x46, 0x46, 0x1A, 0x00, 0x00, 0x00,
            0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x4C,
            0x0D, 0x00, 0x00, 0x00, 0x2F, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        var fileContent = new ByteArrayContent(webpData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/webp");
        form.Add(fileContent, "Image", "test.webp");

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/image", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        Assert.NotNull(result);
        Assert.EndsWith(".webp", result!.ImagePath);
    }
}
