using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Worker.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Register DbContext
builder.AddSqlServerDbContext<IBTS2026Context>("IBTS2026");

// Register Application layer services (handlers, validators, dispatcher)
builder.Services.AddApplication();

// Register Infrastructure layer services (repositories, UoW, query handlers)
builder.Services.AddInfrastructure();

// Register background jobs
builder.Services.AddHostedService<IncidentStatusCheckJob>();
builder.Services.AddHostedService<NotificationProcessorJob>();

var host = builder.Build();

// Apply pending migrations before starting background jobs
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IBTS2026Context>();
    dbContext.Database.Migrate();
}

host.Run();
