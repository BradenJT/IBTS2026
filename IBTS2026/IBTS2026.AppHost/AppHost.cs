var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("IBTS2026");

var apiService = builder.AddProject<Projects.IBTS2026_ApiService>("apiservice")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.IBTS2026_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.IBTS2026_Worker>("worker")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database);

builder.Build().Run();
