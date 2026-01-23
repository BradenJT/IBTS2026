using IBTS2026.ApiService.Endpoints.Users;
using IBTS2026.Application;
using IBTS2026.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register Application layer services (handlers, validators, dispatcher)
builder.Services.AddApplication();

// Register Infrastructure layer services (DbContext, repositories, UoW, query handlers)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map User endpoints
UserEndpoints.Map(app);

app.MapDefaultEndpoints();

app.Run();
