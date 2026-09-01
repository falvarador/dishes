using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.UpdateProfile;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UpdateProfileHandlerUnitTests
{
    [Fact]
    public void UpdateProfileHandler_ApplyProfileUpdates_UpdatesAllFields()
    {
        // Arrange
        var user = new AppIdentityUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Original Name",
            CulinaryTitle = "Sous Chef",
            Biography = "Original bio",
            Location = "London",
            ProfilePhotoUrl = "https://example.com/old.jpg"
        };

        var updateRequest = new UpdateProfileRequest
        {
            FullName = "Updated Name",
            CulinaryTitle = "Executive Chef",
            Biography = "Updated biography",
            Location = "Paris",
            ProfilePhotoUrl = "https://example.com/new.jpg"
        };

        // Act
        var methodInfo = typeof(UpdateProfileHandler).GetMethod("ApplyProfileUpdates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        methodInfo?.Invoke(null, new object[] { user, updateRequest });

        // Assert
        Assert.Equal("Updated Name", user.FullName);
        Assert.Equal("Executive Chef", user.CulinaryTitle);
        Assert.Equal("Updated biography", user.Biography);
        Assert.Equal("Paris", user.Location);
        Assert.Equal("https://example.com/new.jpg", user.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateProfileHandler_ApplyProfileUpdates_IgnoresNullValues()
    {
        // Arrange
        const string originalName = "Original Name";
        const string originalTitle = "Original Title";

        var user = new AppIdentityUser
        {
            Id = "user-456",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = originalName,
            CulinaryTitle = originalTitle,
            Biography = "Original bio",
            Location = "London",
            ProfilePhotoUrl = "https://example.com/old.jpg"
        };

        var updateRequest = new UpdateProfileRequest
        {
            FullName = null,  // Should be ignored
            CulinaryTitle = null,  // Should be ignored
            Biography = "Updated biography",
            Location = null,  // Should be ignored
            ProfilePhotoUrl = "https://example.com/new.jpg"
        };

        // Act
        var methodInfo = typeof(UpdateProfileHandler).GetMethod("ApplyProfileUpdates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        methodInfo?.Invoke(null, new object[] { user, updateRequest });

        // Assert
        Assert.Equal(originalName, user.FullName);  // Should not change
        Assert.Equal(originalTitle, user.CulinaryTitle);  // Should not change
        Assert.Equal("Updated biography", user.Biography);  // Updated
        Assert.Equal("London", user.Location);  // Should not change
        Assert.Equal("https://example.com/new.jpg", user.ProfilePhotoUrl);  // Updated
    }

    [Fact]
    public void UpdateProfileHandler_ApplyProfileUpdates_IgnoresEmptyStrings()
    {
        // Arrange
        const string originalName = "Original Name";

        var user = new AppIdentityUser
        {
            Id = "user-789",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = originalName,
            CulinaryTitle = "Original Title",
            Biography = string.Empty,
            Location = "London",
            ProfilePhotoUrl = string.Empty
        };

        var updateRequest = new UpdateProfileRequest
        {
            FullName = string.Empty,  // Should be ignored
            CulinaryTitle = "New Title",
            Biography = string.Empty,  // Should be ignored
            Location = string.Empty,  // Should be ignored
            ProfilePhotoUrl = "https://example.com/photo.jpg"
        };

        // Act
        var methodInfo = typeof(UpdateProfileHandler).GetMethod("ApplyProfileUpdates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        methodInfo?.Invoke(null, new object[] { user, updateRequest });

        // Assert
        Assert.Equal(originalName, user.FullName);  // Should not change
        Assert.Equal("New Title", user.CulinaryTitle);  // Updated
        Assert.Empty(user.Biography);  // Should not change
        Assert.Equal("London", user.Location);  // Should not change
        Assert.Equal("https://example.com/photo.jpg", user.ProfilePhotoUrl);  // Updated
    }

    [Fact]
    public void UpdateProfileHandler_ApplyProfileUpdates_PartialUpdate()
    {
        // Arrange
        var user = new AppIdentityUser
        {
            Id = "user-999",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Original Name",
            CulinaryTitle = "Chef",
            Biography = "Original bio",
            Location = "City",
            ProfilePhotoUrl = "https://example.com/old.jpg"
        };

        var updateRequest = new UpdateProfileRequest
        {
            FullName = "Updated Name",
            CulinaryTitle = null,
            Biography = null,
            Location = null,
            ProfilePhotoUrl = null
        };

        // Act
        var methodInfo = typeof(UpdateProfileHandler).GetMethod("ApplyProfileUpdates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        methodInfo?.Invoke(null, new object[] { user, updateRequest });

        // Assert
        Assert.Equal("Updated Name", user.FullName);  // Updated
        Assert.Equal("Chef", user.CulinaryTitle);  // Should not change
        Assert.Equal("Original bio", user.Biography);  // Should not change
        Assert.Equal("City", user.Location);  // Should not change
        Assert.Equal("https://example.com/old.jpg", user.ProfilePhotoUrl);  // Should not change
    }

    [Fact]
    public void UpdateProfileRequest_Validation_AcceptsValidMaxLengths()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            FullName = new string('A', 255),  // Max 255
            CulinaryTitle = new string('B', 100),  // Max 100
            Biography = new string('C', 1000),  // Max 1000
            Location = new string('D', 200),  // Max 200
            ProfilePhotoUrl = new string('E', 500)  // Max 500
        };

        // Act & Assert - No exceptions should be thrown
        Assert.NotNull(request);
        Assert.Equal(255, request.FullName.Length);
        Assert.Equal(100, request.CulinaryTitle.Length);
        Assert.Equal(1000, request.Biography.Length);
        Assert.Equal(200, request.Location.Length);
        Assert.Equal(500, request.ProfilePhotoUrl.Length);
    }

    [Fact]
    public void UpdateProfileRequest_Validation_ExceedsFullNameLength()
    {
        // Arrange - Create a request that exceeds max length
        var request = new UpdateProfileRequest
        {
            FullName = new string('A', 256)  // Exceeds 255
        };

        // Act & Assert - Validation attribute should flag this
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileRequest.FullName)));
    }

    [Fact]
    public void UpdateProfileRequest_Validation_ExceedsCulinaryTitleLength()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            CulinaryTitle = new string('B', 101)  // Exceeds 100
        };

        // Act & Assert
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileRequest.CulinaryTitle)));
    }

    [Fact]
    public void UpdateProfileRequest_Validation_ExceedsBiographyLength()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            Biography = new string('C', 1001)  // Exceeds 1000
        };

        // Act & Assert
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileRequest.Biography)));
    }

    [Fact]
    public void UpdateProfileRequest_Validation_ExceedsLocationLength()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            Location = new string('D', 201)  // Exceeds 200
        };

        // Act & Assert
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileRequest.Location)));
    }

    [Fact]
    public void UpdateProfileRequest_Validation_ExceedsPhotoUrlLength()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            ProfilePhotoUrl = new string('E', 501)  // Exceeds 500
        };

        // Act & Assert
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileRequest.ProfilePhotoUrl)));
    }
}
