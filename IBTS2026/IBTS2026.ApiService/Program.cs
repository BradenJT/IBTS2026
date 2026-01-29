using System.Text;
using IBTS2026.ApiService.Endpoints.Auth;
using IBTS2026.ApiService.Endpoints.IncidentNotes;
using IBTS2026.ApiService.Endpoints.Incidents;
using IBTS2026.ApiService.Endpoints.Lookups;
using IBTS2026.ApiService.Endpoints.Users;
using IBTS2026.ApiService.Middleware;
using IBTS2026.Application;
using IBTS2026.Infrastructure;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
        ClockSkew = TimeSpan.Zero
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map endpoints
AuthEndpoints.Map(app);
UserEndpoints.Map(app);
IncidentEndpoints.Map(app);
IncidentNoteEndpoints.Map(app);
LookupEndpoints.Map(app);

app.MapDefaultEndpoints();

app.Run();
