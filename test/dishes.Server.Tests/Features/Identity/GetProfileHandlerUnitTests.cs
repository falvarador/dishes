using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.GetProfile;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class GetProfileHandlerUnitTests
{
    [Fact]
    public void GetProfileHandler_MapToResponse_ReturnsCorrectMapping()
    {
        // Arrange
        var user = new AppIdentityUser
        {
            Id = "test-id-123",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test Full Name",
            CulinaryTitle = "Head Chef",
            Biography = "An experienced chef",
            Location = "New York",
            ProfilePhotoUrl = "https://example.com/photo.jpg",
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act - Using reflection to call private method since it's private in handler
        var methodInfo = typeof(GetProfileHandler).GetMethod("MapToResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var response = methodInfo?.Invoke(null, new object[] { user }) as GetProfileResponse;

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-id-123", response.Id);
        Assert.Equal("testuser", response.UserName);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("Test Full Name", response.FullName);
        Assert.Equal("Head Chef", response.CulinaryTitle);
        Assert.Equal("An experienced chef", response.Biography);
        Assert.Equal("New York", response.Location);
        Assert.Equal("https://example.com/photo.jpg", response.ProfilePhotoUrl);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc), response.CreatedAt);
    }

    [Fact]
    public void GetProfileHandler_MapToResponse_HandlesEmptyProfileFields()
    {
        // Arrange
        var user = new AppIdentityUser
        {
            Id = "empty-id",
            UserName = "emptyuser",
            Email = "empty@example.com",
            FullName = string.Empty,
            CulinaryTitle = string.Empty,
            Biography = string.Empty,
            Location = string.Empty,
            ProfilePhotoUrl = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var methodInfo = typeof(GetProfileHandler).GetMethod("MapToResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var response = methodInfo?.Invoke(null, new object[] { user }) as GetProfileResponse;

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.FullName);
        Assert.Empty(response.CulinaryTitle);
        Assert.Empty(response.Biography);
        Assert.Empty(response.Location);
        Assert.Empty(response.ProfilePhotoUrl);
    }

    [Fact]
    public void GetProfileHandler_MapToResponse_PreservesNullValues()
    {
        // Arrange
        var user = new AppIdentityUser
        {
            Id = "null-test",
            UserName = "nulluser",
            Email = null,
            FullName = "Still Has Name",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var methodInfo = typeof(GetProfileHandler).GetMethod("MapToResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var response = methodInfo?.Invoke(null, new object[] { user }) as GetProfileResponse;

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Email);
        Assert.Equal("Still Has Name", response.FullName);
    }
}
