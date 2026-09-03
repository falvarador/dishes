using dishes.Server.Features.Account.AccountSettings;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace dishes.Server.Tests.Features.Account;

public class AccountSettingsResponseUnitTests
{
    [Fact]
    public void Constructor_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var email = "user@example.com";
        var userName = "testuser";
        var fullName = "John Doe";
        var culinaryTitle = "Chef Profesional";
        var biography = "Passionate about cooking";
        var location = "Madrid, Spain";
        var photoUrl = "https://example.com/photo.jpg";
        var createdAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var response = new AccountSettingsResponse(
            id,
            email,
            userName,
            fullName,
            culinaryTitle,
            biography,
            location,
            photoUrl,
            createdAt
        );

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(email, response.Email);
        Assert.Equal(userName, response.UserName);
        Assert.Equal(fullName, response.FullName);
        Assert.Equal(culinaryTitle, response.CulinaryTitle);
        Assert.Equal(biography, response.Biography);
        Assert.Equal(location, response.Location);
        Assert.Equal(photoUrl, response.ProfilePhotoUrl);
        Assert.Equal(createdAt, response.CreatedAt);
    }

    [Fact]
    public void AccountSettingsResponse_WithEmptyOptionalFields_InitializesCorrectly()
    {
        // Act
        var response = new AccountSettingsResponse(
            "user-id",
            "user@example.com",
            "username",
            "Full Name",
            "",
            "",
            "",
            "",
            DateTime.UtcNow
        );

        // Assert
        Assert.Equal("", response.CulinaryTitle);
        Assert.Equal("", response.Biography);
        Assert.Equal("", response.Location);
        Assert.Equal("", response.ProfilePhotoUrl);
    }

    [Fact]
    public void AccountSettingsResponse_CanBeSerializedToJson()
    {
        // Arrange
        var response = new AccountSettingsResponse(
            "user-123",
            "chef@example.com",
            "chefuser",
            "Marco Rossi",
            "Executive Chef",
            "Italian cuisine specialist",
            "Milan, Italy",
            "https://example.com/marco.jpg",
            DateTime.UtcNow.AddDays(-30)
        );

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("Marco Rossi", json);
        Assert.Contains("Executive Chef", json);
        Assert.Contains("chef@example.com", json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void AccountSettingsResponse_CanBeDeserializedFromJson()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var json = $@"{{
            ""id"": ""user-456"",
            ""email"": ""lucia@example.com"",
            ""userName"": ""lucia_chef"",
            ""fullName"": ""Lucia Rossi"",
            ""culinaryTitle"": ""Pastry Chef"",
            ""biography"": ""Specialized in artisan pastries"",
            ""location"": ""Rome, Italy"",
            ""profilePhotoUrl"": ""https://example.com/lucia.jpg"",
            ""createdAt"": ""{now:O}""
        }}";

        // Act
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = System.Text.Json.JsonSerializer.Deserialize<AccountSettingsResponse>(json, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("user-456", response.Id);
        Assert.Equal("lucia@example.com", response.Email);
        Assert.Equal("Lucia Rossi", response.FullName);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("María García")]
    [InlineData("李明")]
    public void AccountSettingsResponse_WithDifferentFullNames_StoresCorrectly(string fullName)
    {
        // Act
        var response = new AccountSettingsResponse(
            "id",
            "email@example.com",
            "username",
            fullName,
            "",
            "",
            "",
            "",
            DateTime.UtcNow
        );

        // Assert
        Assert.Equal(fullName, response.FullName);
    }
}

public class UpdateAccountSettingsRequestUnitTests
{
    [Fact]
    public void Constructor_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var fullName = "John Doe";
        var culinaryTitle = "Head Chef";
        var biography = "Experienced in French cuisine";
        var location = "Paris, France";
        var photoUrl = "https://example.com/photo.jpg";

        // Act
        var request = new UpdateAccountSettingsRequest(
            fullName,
            culinaryTitle,
            biography,
            location,
            photoUrl
        );

        // Assert
        Assert.Equal(fullName, request.FullName);
        Assert.Equal(culinaryTitle, request.CulinaryTitle);
        Assert.Equal(biography, request.Biography);
        Assert.Equal(location, request.Location);
        Assert.Equal(photoUrl, request.ProfilePhotoUrl);
    }

    [Fact]
    public void Constructor_WithOnlyFullName_InitializesWithDefaults()
    {
        // Act
        var request = new UpdateAccountSettingsRequest("Jane Smith");

        // Assert
        Assert.Equal("Jane Smith", request.FullName);
        Assert.Equal("", request.CulinaryTitle);
        Assert.Equal("", request.Biography);
        Assert.Equal("", request.Location);
        Assert.Equal("", request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateAccountSettingsRequest_WithMaxLengthValues_InitializesCorrectly()
    {
        // Arrange
        var maxFullName = new string('a', 500);
        var maxCulinaryTitle = new string('b', 200);
        var maxBiography = new string('c', 2000);
        var maxLocation = new string('d', 200);
        var maxPhotoUrl = "https://example.com/" + new string('e', 472); // Max 500

        // Act
        var request = new UpdateAccountSettingsRequest(
            maxFullName,
            maxCulinaryTitle,
            maxBiography,
            maxLocation,
            maxPhotoUrl
        );

        // Assert
        Assert.Equal(500, request.FullName.Length);
        Assert.Equal(200, request.CulinaryTitle.Length);
        Assert.Equal(2000, request.Biography.Length);
        Assert.Equal(200, request.Location.Length);
    }

    [Fact]
    public void UpdateAccountSettingsRequest_CanBeSerializedToJson()
    {
        // Arrange
        var request = new UpdateAccountSettingsRequest(
            "Marco Rossi",
            "Executive Chef",
            "Italian cuisine specialist",
            "Milan",
            "https://example.com/profile.jpg"
        );

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("Marco Rossi", json);
        Assert.Contains("Executive Chef", json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void UpdateAccountSettingsRequest_CanBeDeserializedFromJson()
    {
        // Arrange
        var json = @"{
            ""fullName"": ""Sofia Martínez"",
            ""culinaryTitle"": ""Sous Chef"",
            ""biography"": ""Loves Spanish gastronomy"",
            ""location"": ""Barcelona, Spain"",
            ""profilePhotoUrl"": ""https://example.com/sofia.jpg""
        }";

        // Act
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var request = System.Text.Json.JsonSerializer.Deserialize<UpdateAccountSettingsRequest>(json, options);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("Sofia Martínez", request.FullName);
        Assert.Equal("Sous Chef", request.CulinaryTitle);
        Assert.Equal("Barcelona, Spain", request.Location);
    }

    [Theory]
    [InlineData("https://example.com/photo.jpg")]
    [InlineData("https://cdn.example.com/images/profile.png")]
    [InlineData("")]
    public void UpdateAccountSettingsRequest_WithValidUrls_StoresCorrectly(string url)
    {
        // Act
        var request = new UpdateAccountSettingsRequest(
            "Full Name",
            "",
            "",
            "",
            url
        );

        // Assert
        Assert.Equal(url, request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateAccountSettingsRequest_WithMultipleFieldsUpdated_MaintainsAllValues()
    {
        // Arrange
        var requests = new List<UpdateAccountSettingsRequest>
        {
            new("User One", "Chef", "", "", ""),
            new("User Two", "", "Bio", "", ""),
            new("User Three", "", "", "Location", "")
        };

        // Act & Assert
        Assert.Equal("User One", requests[0].FullName);
        Assert.Equal("Chef", requests[0].CulinaryTitle);
        Assert.Equal("User Two", requests[1].FullName);
        Assert.Equal("Bio", requests[1].Biography);
    }
}
