using dishes.Server.Features.Account.AccountSettings;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Account;

public class AccountSettingsIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public AccountSettingsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetAccountSettings_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/account/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccountSettings_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UpdateAccountSettingsRequest(
            "Updated Name",
            "Chef",
            "Biography",
            "Location",
            ""
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/account/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
