using System.Net;
using System.Net.Http.Json;
using Oms.Application.Features.Tenants.Commands.RegisterTenant;

namespace Oms.WebApi.IntegrationTests.tenants;

public class TenantRegistrationTests : IClassFixture<OmsTestWebApplicationFactory>
{
    private readonly OmsTestWebApplicationFactory _factory;
    
    public TenantRegistrationTests(OmsTestWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task RegisterTenant_ValidRequest_Returns201CreatedAndCreatesRecords()
    {
        var client = _factory.CreateClient();
        
        // Arrange
        var command = new RegisterTenantCommand(
            CompanyName: "Acme Corp",
            OwnerFirstName: "Salindu",
            OwnerLastName: "Hansaka",
            OwnerEmail: "salindu.h@acme.com",
            OwnerPassword: "P@ssw0rd"
        );
        
        // Act
        var response = await client.PostAsJsonAsync("/api/tenants/register", command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<TenantRegistrationResult>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.TenantId);
        Assert.NotEqual(Guid.Empty, result.OwnerId);
        Assert.Equal("acme-corp", result.Slug);
    }
    
    [Fact]
    public async Task RegisterTenant_DuplicateSlug_Returns400BadRequest()
    {
        var client = _factory.CreateClient();
        
        // Arrange: Try to register the exact same company name twice
        var command1 = new RegisterTenantCommand("Unique Corp", "UserOne", "Test", "one@unique.com", "P@ssw0rd");
        var command2 = new RegisterTenantCommand("Unique Corp", "UserTwo", "Test", "two@unique.com", "P@ssw0rd");
        
        // Act
        var response1 = await client.PostAsJsonAsync("/api/tenants/register", command1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        
        var response2 = await client.PostAsJsonAsync("/api/tenants/register", command2);
        // Assert: The second register must fail due to slug uniqueness
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
}