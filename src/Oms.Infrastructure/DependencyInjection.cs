using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oms.Application.Common.Interfaces;
using Oms.Infrastructure.Persistence;
using Oms.Infrastructure.Security;

namespace Oms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConfiguration");
        
        // 1. Register the concrete DbContext with Npgsql
        services.AddDbContext<OmsDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Oms.Infrastructure")));
        
        // 2. Bind the Interface to our concrete class as a Scoped dependency
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<OmsDbContext>());
        
        // Register password hasher
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        
        // JWT
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtProvider, JwtProvider>();
        
        return services;
    }
}