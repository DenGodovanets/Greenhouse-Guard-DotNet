using System.Text.Json.Serialization;
using GreenhouseGuard.Api.Middleware;
using GreenhouseGuard.Application;
using GreenhouseGuard.Application.Configuration;
using GreenhouseGuard.Infrastructure;
using GreenhouseGuard.Infrastructure.Hubs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict; });
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});
builder.Services.AddOpenApi();
builder.Services.Configure<SensorMonitoringOptions>(
    builder.Configuration.GetSection(SensorMonitoringOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapHub<SensorHub>(SensorHub.HubPath);

app.Run();