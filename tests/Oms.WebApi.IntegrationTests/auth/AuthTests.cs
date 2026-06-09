using System.Net;
using System.Net.Http.Json;
using Oms.Application.Features.Auth.Commands.Login;
using Oms.Application.Features.Auth.Commands.RefreshToken;
using Oms.Application.Features.Auth.Commands.Logout;
using Oms.Application.Features.Tenants.Commands.RegisterTenant;
using Oms.WebApi.Endpoints;

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
        Assert.NotNull(result!.AccessToken);
        Assert.NotNull(result!.RefreshToken);
    }

    [Fact]
    public async Task Login_WithIncorrectCredentials_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();
        
        // 1. Register a Tenant to seed a user
        var email = $"owner-{Guid.NewGuid()}@test.com";
        var registerCommand = new RegisterTenantCommand("Auth Test Co testing for login", "Test", "User", email, "SuperSecure123!");
        await client.PostAsJsonAsync("/api/tenants/register", registerCommand);
        
        // 2. Perform Login with incorrect credentials
        var loginCommand = new LoginCommand(registerCommand.OwnerEmail, "WrongPassword");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.False(result!.IsSuccess);
        Assert.Null(result!.AccessToken);
        Assert.Null(result!.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_RotatesTokens()
    {
        var client = _factory.CreateClient();
        
        // 1. Register a Tenant to seed a user
        var email = $"owner-{Guid.NewGuid()}@test.com";
        var registerCommand = new RegisterTenantCommand("Auth Test Co testing for refresh", "Test", "User", email, "SuperSecure123!");
        await client.PostAsJsonAsync("/api/tenants/register", registerCommand);
        
        // 2. Perform Login to get the initial tokens
        var loginCommand = new LoginCommand(registerCommand.OwnerEmail, registerCommand.OwnerPassword);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        
        var originalRefreshToken = loginResult!.RefreshToken;
        Assert.NotNull(originalRefreshToken);

        // 3. Request Token Refresh using the original Refresh Token
        var refreshRequest = new AuthEndpoints.RefreshRequest(originalRefreshToken);
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refreshToken", refreshRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResult>();
        Assert.True(refreshResult!.IsSuccess);
        Assert.NotNull(refreshResult.AccessToken);
        Assert.NotNull(refreshResult.RefreshToken);
        
        // Token Rotation Check: The new refresh token should be completely different from the original one
        Assert.NotEqual(originalRefreshToken, refreshResult.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        // 1. Send an invalid/non-existent refresh token to refresh endpoint
        var refreshRequest = new AuthEndpoints.RefreshRequest("non-existent-token-value");
        var response = await client.PostAsJsonAsync("/api/auth/refreshToken", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResult>();
        Assert.False(result!.IsSuccess);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_RevokesTokenAndInvalidatesSubsequentRefreshCalls()
    {
        var client = _factory.CreateClient();
        
        // 1. Register a Tenant to seed a user
        var email = $"owner-{Guid.NewGuid()}@test.com";
        var registerCommand = new RegisterTenantCommand("Auth Test Co testing for logout", "Test", "User", email, "SuperSecure123!");
        await client.PostAsJsonAsync("/api/tenants/register", registerCommand);
        
        // 2. Perform Login to get the tokens
        var loginCommand = new LoginCommand(registerCommand.OwnerEmail, registerCommand.OwnerPassword);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        
        var refreshToken = loginResult!.RefreshToken;
        Assert.NotNull(refreshToken);

        // 3. Logout using the Refresh Token
        var logoutRequest = new AuthEndpoints.LogoutRequest(refreshToken);
        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", logoutRequest);
        
        // Assert logout succeeds
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        var logoutResult = await logoutResponse.Content.ReadFromJsonAsync<LogoutResult>();
        Assert.True(logoutResult!.IsSuccess);

        // 4. Try to refresh using the logged-out (revoked) Refresh Token
        var refreshRequest = new AuthEndpoints.RefreshRequest(refreshToken);
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refreshToken", refreshRequest);
        
        // Assert refresh fails with 401 Unauthorized because the token was revoked during logout
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ReturnsOk()
    {
        var client = _factory.CreateClient();

        // 1. Logout using an invalid/non-existent refresh token
        var logoutRequest = new AuthEndpoints.LogoutRequest("non-existent-token-value");
        var response = await client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert logout still succeeds (idempotent behavior)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LogoutResult>();
        Assert.True(result!.IsSuccess);
    }
}