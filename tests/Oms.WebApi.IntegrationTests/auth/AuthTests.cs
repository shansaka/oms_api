using System.Net;
using System.Net.Http.Json;
using Oms.Application.Features.Auth.Commands.Login;
using Oms.Application.Features.Tenants.Commands.RegisterTenant;

namespace Oms.WebApi.IntegrationTests.auth;

public class AuthTests : IClassFixture<OmsTestWebApplicationFactory>
{
    private readonly OmsTestWebApplicationFactory _factory;

    public AuthTests(OmsTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOk()
    {
        var client = _factory.CreateClient();
        
        // 1. Register a Tenant to seed a user
        var email = $"owner-{Guid.NewGuid()}@test.com";
        var registerCommand = new RegisterTenantCommand("Auth Test Co testing for login", "Test", "User", email, "SuperSecure123!");
        await client.PostAsJsonAsync("/api/tenants/register", registerCommand);
        
        // 2. Perform Login with correct credentials
        var loginCommand = new LoginCommand(registerCommand.OwnerEmail, registerCommand.OwnerPassword);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result!.UserId);
    }

    [Fact]
    public async Task Login_WithIncorrectCredentials_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();
        
        // 1. Register a Tenant to seed a user
        var email = $"owner-{Guid.NewGuid()}@test.com";
        var registerCommand = new RegisterTenantCommand("Auth Test Co testing for login", "Test", "User", email, "SuperSecure123!");
        await client.PostAsJsonAsync("/api/tenants/register", registerCommand);
        
        // 2. Perform Login with correct credentials
        var loginCommand = new LoginCommand(registerCommand.OwnerEmail, "WrongPassword");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.False(result!.IsSuccess);
        Assert.Null(result!.UserId);
    }
}