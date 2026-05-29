using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
namespace Oms.WebApi.IntegrationTests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Get_HealthCheck_ReturnsHealthyAndOk()
    {
        // 1. Arrange: Create an in-memory client
        var client = _factory.CreateClient();
        
        // 2. Act: Send GET request to /health
        var response = await client.GetAsync("/health");
        
        // 3. Assert: Verify the HTTP response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }
}