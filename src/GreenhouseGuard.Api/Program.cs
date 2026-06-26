using Scalar.AspNetCore;
using GreenhouseGuard.Application.Configuration;
using GreenhouseGuard.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<SensorMonitoringOptions>(
    builder.Configuration.GetSection(SensorMonitoringOptions.SectionName));
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();