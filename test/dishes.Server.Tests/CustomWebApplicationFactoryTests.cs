namespace dishes.Server.Tests;

public class CustomWebApplicationFactoryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CustomWebApplicationFactoryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateClient_ReturnsWorkingClient()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dishes");

        response.EnsureSuccessStatusCode();
    }
}
