using System.Net.Mime;

namespace dishes.Server.Services;

public interface IImageUploadService
{
    Task<string> UploadRecipeImageAsync(IFormFile file, Guid recipeId, CancellationToken cancellationToken);
    Task DeleteRecipeImageAsync(string imagePath, CancellationToken cancellationToken);
    Task CleanupTestImagesAsync(CancellationToken cancellationToken);
}

public class ImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;
    private const long MaxFileSizeBytes = 1024 * 1024; // 1MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = 
    { 
        "image/jpeg", 
        "image/png", 
        "image/gif", 
        "image/webp" 
    };
    private const string ImagesDirectory = "images";
    private const string RecipeImagesSubdirectory = "recipes";

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadRecipeImageAsync(IFormFile file, Guid recipeId, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required", nameof(file));
        }

        // Validate file size
        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of 1MB. Actual size: {file.Length / 1024}KB");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"File format '{fileExtension}' is not allowed. Supported formats: {string.Join(", ", AllowedExtensions)}");
        }

        // Validate MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType ?? string.Empty))
        {
            throw new InvalidOperationException($"MIME type '{file.ContentType}' is not allowed. Supported types: {string.Join(", ", AllowedMimeTypes)}");
        }

        try
        {
            // Generate unique filename
            var timestamp = DateTime.UtcNow.Ticks;
            var filename = $"{recipeId}_{timestamp}{fileExtension}";

            // Build the directory path
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", ImagesDirectory, RecipeImagesSubdirectory);

            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, filename);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return relative path for storage in database
            var relativePath = Path.Combine("/", ImagesDirectory, RecipeImagesSubdirectory, filename).Replace("\\", "/");

            _logger.LogInformation("Image uploaded successfully for recipe {RecipeId}: {FilePath}", recipeId, relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for recipe {RecipeId}", recipeId);
            throw;
        }
    }

    public async Task DeleteRecipeImageAsync(string imagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        try
        {
            // Convert relative path to absolute file path
            var uploadPath = imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", uploadPath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted successfully: {FilePath}", imagePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImagePath}", imagePath);
            // Don't throw - we don't want deletion failures to break the application
        }

        await Task.CompletedTask;
    }

    public async Task CleanupTestImagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", ImagesDirectory, RecipeImagesSubdirectory);

            if (Directory.Exists(uploadsPath))
            {
                var files = Directory.GetFiles(uploadsPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                _logger.LogInformation("Cleaned up {FileCount} test images", files.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up test images");
            // Don't throw - we don't want cleanup failures to break tests
        }

        await Task.CompletedTask;
    }
}
