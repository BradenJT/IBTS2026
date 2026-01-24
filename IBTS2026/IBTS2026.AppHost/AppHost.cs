var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("IBTS2026");

var apiService = builder.AddProject<Projects.IBTS2026_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.IBTS2026_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.IBTS2026_Worker>("worker")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
