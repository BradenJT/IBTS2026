using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Worker.Jobs;

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

var host = builder.Build();
host.Run();
