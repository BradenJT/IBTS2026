using IBTS2026.ApiService.Middleware;
using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

// Configure NLog early to catch startup errors
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Debug("Starting IBTS2026 API Service");

    // Disable automatic claim type mapping so JWT claims are preserved as-is
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog
    builder.ConfigureNLog();

    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Configure JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key is not configured. Add Jwt:Key to appsettings.json");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IBTS2026";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IBTS2026";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            NameClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier"
        };
    });

    // Configure Authorization Policies
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
        .AddPolicy("RequireUserRole", policy => policy.RequireRole("Admin", "User"));

    // Register DbContext with Aspire SQL Server integration
    builder.AddSqlServerDbContext<IBTS2026Context>("IBTS2026");

    // Register Application layer services (handlers, validators, dispatcher)
    builder.Services.AddApplication();

    // Register Infrastructure layer services (repositories, UoW, query handlers)
    builder.Services.AddInfrastructure();

    // Register Controllers
    builder.Services.AddControllers();

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

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Add auth logging middleware for debugging (after auth so claims are populated)
    app.UseAuthLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Map endpoints
    app.MapControllers();

    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application exit
    LogManager.Shutdown();
}
