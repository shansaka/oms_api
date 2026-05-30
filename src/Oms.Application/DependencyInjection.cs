using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Oms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Register FluentValidation validators automatically from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // 2. Register MediatR and automatically scan this assembly for handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}