using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Oms.Application.Features.Auth.Commands.Login;
using Oms.Application.Features.Tenants.Commands.RegisterTenant;
using Oms.Application.Features.Users.Commands;
using Oms.Application.Features.Users.Queries;

namespace Oms.WebApi.IntegrationTests.users;

public class UserManagementTests : IClassFixture<OmsTestWebApplicationFactory>
{
    private readonly OmsTestWebApplicationFactory _factory;
    public UserManagementTests(OmsTestWebApplicationFactory factory)
    {
        _factory = factory;
    }
    private async Task<(HttpClient Client, string Token, Guid OwnerId, Guid TenantId)> RegisterAndLoginTenantAsync(string companyName)
    {
        var client = _factory.CreateClient();
        var email = $"owner-{Guid.NewGuid()}@test.com";
        
        // 1. Register Tenant (Owner gets users:manage because OmsDbContextSeed maps it to Owner role)
        var registerResponse = await client.PostAsJsonAsync("/api/tenants/register", 
            new RegisterTenantCommand(companyName, "OwnerFirst", "OwnerLast", email, "Password123!"));
        var regResult = await registerResponse.Content.ReadFromJsonAsync<TenantRegistrationResult>();
        
        // 2. Log In to retrieve JWT Token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", 
            new LoginCommand(email, "Password123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        return (client, loginResult!.AccessToken!, regResult!.OwnerId, regResult.TenantId);
    }
    
    [Fact]
    public async Task GetUsers_ReturnsOnlyUsersBelongingToCallersTenant()
    {
        // Arrange: Set up two distinct tenants
        var tenantA = await RegisterAndLoginTenantAsync("Tenant A Corp");
        var tenantB = await RegisterAndLoginTenantAsync("Tenant B Corp");
        
        // Seed an extra staff user under Tenant B
        tenantB.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenantB.Token);
        var createStaffCommand = new CreateStaffUserCommand("StaffB", "User", $"staffB-{Guid.NewGuid()}@test.com", "Password123!");
        await tenantB.Client.PostAsJsonAsync("/api/users/staff", createStaffCommand);
        
        // Act: Request users list authenticated as Tenant A
        tenantA.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.Token);
        var response = await tenantA.Client.GetAsync("/api/users");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var usersList = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(usersList);
        
        // Tenant A must only see Tenant A's owner, not Tenant B's owner or Tenant B's staff!
        Assert.All(usersList, user => Assert.NotEqual(tenantB.OwnerId, user.Id));
        Assert.Contains(usersList, user => user.Id == tenantA.OwnerId);
    }
    
    [Fact]
    public async Task CreateStaffUser_Returns201CreatedAndSavesCorrectly()
    {
        // Arrange
        var tenantA = await RegisterAndLoginTenantAsync("Tenant C Corp");
        tenantA.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.Token);
        var staffCommand = new CreateStaffUserCommand(
            FirstName: "Jane",
            LastName: "Doe",
            Email: $"jane.doe-{Guid.NewGuid()}@tenantA.com",
            Password: "SecurePassword123!"
        );
        
        // Act
        var response = await tenantA.Client.PostAsJsonAsync("/api/users/staff", staffCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateStaffUserResult>();
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(staffCommand.Email, result.Email);
        Assert.Equal("Staff", result.Role);
    }
}