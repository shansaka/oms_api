using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oms.Infrastructure; 
using Oms.WebApi;
using Serilog;
using Oms.WebApi.Endpoints;
using Oms.Application;
using Oms.Infrastructure.Persistence;
using Oms.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());

//  Register both Application and Infrastructure Services!
builder.Services.AddApplicationServices(builder.Configuration);  
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// Register the dynamic policy provider and handler
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization();



var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OMS Api");
        options.RoutePrefix = "swagger";
    });
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OmsDbContext>();
    await context.Database.MigrateAsync();
    await OmsDbContextSeed.SeedAsync(context);
}

app.UseHttpsRedirection();
app.UseHealthChecks("/health");

// Register all modular endpoints here!
app.MapTenantEndpoints(); 
app.MapAuthEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program { }