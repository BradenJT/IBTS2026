using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Worker.Jobs;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

// Configure NLog early to catch startup errors
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Debug("Starting IBTS2026 Worker Service");

    var builder = Host.CreateApplicationBuilder(args);

    // Configure NLog
    builder.ConfigureNLog();

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
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Worker service stopped due to exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application exit
    LogManager.Shutdown();
}
