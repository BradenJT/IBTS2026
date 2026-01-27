using IBTS2026.ApiService.Endpoints.Incidents;
using IBTS2026.ApiService.Endpoints.Users;
using IBTS2026.ApiService.Middleware;
using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register DbContext with Aspire SQL Server integration
builder.AddSqlServerDbContext<IBTS2026Context>("IBTS2026");

// Register Application layer services (handlers, validators, dispatcher)
builder.Services.AddApplication();

// Register Infrastructure layer services (repositories, UoW, query handlers)
builder.Services.AddInfrastructure();

var app = builder.Build();

// Apply pending migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IBTS2026Context>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseCorrelationId();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map endpoints
UserEndpoints.Map(app);
IncidentEndpoints.Map(app);

app.MapDefaultEndpoints();

app.Run();
