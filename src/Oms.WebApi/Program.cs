using Oms.Infrastructure; 
using Oms.WebApi;
using Serilog;
using Oms.WebApi.Endpoints;
using Oms.Application; 

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

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseHealthChecks("/health");

// Register all modular endpoints here!
app.MapTenantEndpoints(); 

app.Run();

public partial class Program { }