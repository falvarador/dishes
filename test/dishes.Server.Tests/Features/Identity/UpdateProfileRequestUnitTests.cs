using dishes.Server.Features.Identity.UpdateProfile;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UpdateProfileRequestUnitTests
{
    [Fact]
    public void UpdateProfileRequest_DefaultConstructor_AllPropertiesNull()
    {
        // Act
        var request = new UpdateProfileRequest();

        // Assert
        Assert.Null(request.FullName);
        Assert.Null(request.CulinaryTitle);
        Assert.Null(request.Biography);
        Assert.Null(request.Location);
        Assert.Null(request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateProfileRequest_CanSetAllProperties()
    {
        // Arrange
        var request = new UpdateProfileRequest();

        // Act
        request.FullName = "New Name";
        request.CulinaryTitle = "New Title";
        request.Biography = "New Bio";
        request.Location = "New Location";
        request.ProfilePhotoUrl = "https://example.com/new.jpg";

        // Assert
        Assert.Equal("New Name", request.FullName);
        Assert.Equal("New Title", request.CulinaryTitle);
        Assert.Equal("New Bio", request.Biography);
        Assert.Equal("New Location", request.Location);
        Assert.Equal("https://example.com/new.jpg", request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateProfileRequest_SupportsPartialUpdates()
    {
        // Arrange & Act
        var request = new UpdateProfileRequest
        {
            FullName = "Updated Name",
            CulinaryTitle = null,
            Biography = "Updated Bio",
            Location = null,
            ProfilePhotoUrl = null
        };

        // Assert
        Assert.Equal("Updated Name", request.FullName);
        Assert.Null(request.CulinaryTitle);
        Assert.Equal("Updated Bio", request.Biography);
        Assert.Null(request.Location);
        Assert.Null(request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateProfileRequest_ValidationAttribute_FullNameMaxLength()
    {
        // Arrange
        var validRequest = new UpdateProfileRequest { FullName = new string('A', 255) };
        var invalidRequest = new UpdateProfileRequest { FullName = new string('A', 256) };

        // Act & Assert
        var validContext = new System.ComponentModel.DataAnnotations.ValidationContext(validRequest);
        var validResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(validRequest, validContext, validResults, true);

        var invalidContext = new System.ComponentModel.DataAnnotations.ValidationContext(invalidRequest);
        var invalidResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var invalidIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalidRequest, invalidContext, invalidResults, true);

        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
    }

    [Fact]
    public void UpdateProfileRequest_ValidationAttribute_CulinaryTitleMaxLength()
    {
        // Arrange
        var validRequest = new UpdateProfileRequest { CulinaryTitle = new string('B', 100) };
        var invalidRequest = new UpdateProfileRequest { CulinaryTitle = new string('B', 101) };

        // Act & Assert
        var validContext = new System.ComponentModel.DataAnnotations.ValidationContext(validRequest);
        var validResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(validRequest, validContext, validResults, true);

        var invalidContext = new System.ComponentModel.DataAnnotations.ValidationContext(invalidRequest);
        var invalidResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var invalidIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalidRequest, invalidContext, invalidResults, true);

        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
    }

    [Fact]
    public void UpdateProfileRequest_ValidationAttribute_BiographyMaxLength()
    {
        // Arrange
        var validRequest = new UpdateProfileRequest { Biography = new string('C', 1000) };
        var invalidRequest = new UpdateProfileRequest { Biography = new string('C', 1001) };

        // Act & Assert
        var validContext = new System.ComponentModel.DataAnnotations.ValidationContext(validRequest);
        var validResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(validRequest, validContext, validResults, true);

        var invalidContext = new System.ComponentModel.DataAnnotations.ValidationContext(invalidRequest);
        var invalidResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var invalidIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalidRequest, invalidContext, invalidResults, true);

        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
    }

    [Fact]
    public void UpdateProfileRequest_ValidationAttribute_LocationMaxLength()
    {
        // Arrange
        var validRequest = new UpdateProfileRequest { Location = new string('D', 200) };
        var invalidRequest = new UpdateProfileRequest { Location = new string('D', 201) };

        // Act & Assert
        var validContext = new System.ComponentModel.DataAnnotations.ValidationContext(validRequest);
        var validResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(validRequest, validContext, validResults, true);

        var invalidContext = new System.ComponentModel.DataAnnotations.ValidationContext(invalidRequest);
        var invalidResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var invalidIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalidRequest, invalidContext, invalidResults, true);

        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
    }

    [Fact]
    public void UpdateProfileRequest_ValidationAttribute_ProfilePhotoUrlMaxLength()
    {
        // Arrange
        var validRequest = new UpdateProfileRequest { ProfilePhotoUrl = new string('E', 500) };
        var invalidRequest = new UpdateProfileRequest { ProfilePhotoUrl = new string('E', 501) };

        // Act & Assert
        var validContext = new System.ComponentModel.DataAnnotations.ValidationContext(validRequest);
        var validResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(validRequest, validContext, validResults, true);

        var invalidContext = new System.ComponentModel.DataAnnotations.ValidationContext(invalidRequest);
        var invalidResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var invalidIsValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalidRequest, invalidContext, invalidResults, true);

        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
    }

    [Fact]
    public void UpdateProfileRequest_AllowsValidUrls()
    {
        // Arrange & Act
        var request = new UpdateProfileRequest
        {
            ProfilePhotoUrl = "https://example.com/very/long/path/to/profile/photo/image123456789.jpg"
        };

        // Assert
        Assert.NotNull(request.ProfilePhotoUrl);
        Assert.StartsWith("https://", request.ProfilePhotoUrl);
    }

    [Fact]
    public void UpdateProfileRequest_AllValidDataPasses()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            FullName = "John Doe",
            CulinaryTitle = "Executive Chef",
            Biography = "Experienced chef with passion for cooking",
            Location = "Paris, France",
            ProfilePhotoUrl = "https://example.com/john.jpg"
        };

        // Act
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void UpdateProfileRequest_EmptyStringsAllowed()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            FullName = string.Empty,
            CulinaryTitle = string.Empty,
            Biography = string.Empty,
            Location = string.Empty,
            ProfilePhotoUrl = string.Empty
        };

        // Act
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void UpdateProfileRequest_MultipleValidationErrors()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            FullName = new string('A', 256),  // Too long
            CulinaryTitle = new string('B', 101),  // Too long
            Biography = new string('C', 1001),  // Too long
            Location = new string('D', 201),  // Too long
            ProfilePhotoUrl = new string('E', 501)  // Too long
        };

        // Act
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Equal(5, results.Count);  // Should have 5 validation errors
    }
}
