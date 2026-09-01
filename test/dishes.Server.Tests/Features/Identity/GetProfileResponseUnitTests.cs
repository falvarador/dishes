using dishes.Server.Features.Identity.GetProfile;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class GetProfileResponseUnitTests
{
    [Fact]
    public void GetProfileResponse_DefaultConstructor_InitializesEmptyFields()
    {
        // Act
        var response = new GetProfileResponse();

        // Assert
        Assert.Null(response.Id);
        Assert.Null(response.Email);
        Assert.Null(response.UserName);
        Assert.Equal(string.Empty, response.FullName);
        Assert.Equal(string.Empty, response.CulinaryTitle);
        Assert.Equal(string.Empty, response.Biography);
        Assert.Equal(string.Empty, response.Location);
        Assert.Equal(string.Empty, response.ProfilePhotoUrl);
        Assert.Equal(default, response.CreatedAt);
    }

    [Fact]
    public void GetProfileResponse_CanSetAllProperties()
    {
        // Arrange
        var response = new GetProfileResponse();
        var createdAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        response.Id = "user-123";
        response.Email = "test@example.com";
        response.UserName = "testuser";
        response.FullName = "Test User";
        response.CulinaryTitle = "Chef";
        response.Biography = "A test bio";
        response.Location = "Test City";
        response.ProfilePhotoUrl = "https://example.com/photo.jpg";
        response.CreatedAt = createdAt;

        // Assert
        Assert.Equal("user-123", response.Id);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("testuser", response.UserName);
        Assert.Equal("Test User", response.FullName);
        Assert.Equal("Chef", response.CulinaryTitle);
        Assert.Equal("A test bio", response.Biography);
        Assert.Equal("Test City", response.Location);
        Assert.Equal("https://example.com/photo.jpg", response.ProfilePhotoUrl);
        Assert.Equal(createdAt, response.CreatedAt);
    }

    [Fact]
    public void GetProfileResponse_AllowsNullValues()
    {
        // Arrange
        var response = new GetProfileResponse
        {
            Id = null,
            Email = null,
            UserName = null
        };

        // Assert
        Assert.Null(response.Id);
        Assert.Null(response.Email);
        Assert.Null(response.UserName);
    }

    [Fact]
    public void GetProfileResponse_AllowsEmptyStrings()
    {
        // Arrange
        var response = new GetProfileResponse
        {
            FullName = string.Empty,
            CulinaryTitle = string.Empty,
            Biography = string.Empty,
            Location = string.Empty,
            ProfilePhotoUrl = string.Empty
        };

        // Assert
        Assert.Empty(response.FullName);
        Assert.Empty(response.CulinaryTitle);
        Assert.Empty(response.Biography);
        Assert.Empty(response.Location);
        Assert.Empty(response.ProfilePhotoUrl);
    }

    [Fact]
    public void GetProfileResponse_ContainsAllRequiredFields()
    {
        // Arrange
        var response = new GetProfileResponse();

        // Assert - Verify all properties exist and are accessible
        var properties = typeof(GetProfileResponse).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        Assert.Contains("Id", propertyNames);
        Assert.Contains("Email", propertyNames);
        Assert.Contains("UserName", propertyNames);
        Assert.Contains("FullName", propertyNames);
        Assert.Contains("CulinaryTitle", propertyNames);
        Assert.Contains("Biography", propertyNames);
        Assert.Contains("Location", propertyNames);
        Assert.Contains("ProfilePhotoUrl", propertyNames);
        Assert.Contains("CreatedAt", propertyNames);
    }

    [Fact]
    public void GetProfileResponse_SerializableData()
    {
        // Arrange
        var response = new GetProfileResponse
        {
            Id = "user-456",
            Email = "john@example.com",
            UserName = "johndoe",
            FullName = "John Doe",
            CulinaryTitle = "Executive Chef",
            Biography = "Passionate about cooking",
            Location = "Paris",
            ProfilePhotoUrl = "https://example.com/john.jpg",
            CreatedAt = new DateTime(2024, 1, 20, 15, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("Id", json);
        Assert.Contains("user-456", json);
        Assert.Contains("john@example.com", json);
        Assert.Contains("johndoe", json);
        Assert.Contains("John Doe", json);
        Assert.Contains("Executive Chef", json);
    }

    [Fact]
    public void GetProfileResponse_RoundTripSerialization()
    {
        // Arrange
        var original = new GetProfileResponse
        {
            Id = "user-789",
            Email = "test@example.com",
            UserName = "testuser",
            FullName = "Test Full Name",
            CulinaryTitle = "Head Chef",
            Biography = "Test biography",
            Location = "Test Location",
            ProfilePhotoUrl = "https://example.com/test.jpg",
            CreatedAt = new DateTime(2024, 1, 25, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<GetProfileResponse>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Email, deserialized.Email);
        Assert.Equal(original.UserName, deserialized.UserName);
        Assert.Equal(original.FullName, deserialized.FullName);
        Assert.Equal(original.CulinaryTitle, deserialized.CulinaryTitle);
        Assert.Equal(original.Biography, deserialized.Biography);
        Assert.Equal(original.Location, deserialized.Location);
        Assert.Equal(original.ProfilePhotoUrl, deserialized.ProfilePhotoUrl);
        Assert.Equal(original.CreatedAt, deserialized.CreatedAt);
    }
}
