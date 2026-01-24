using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Incidents.GetIncident;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Infrastructure.Queries.Incidents;
using IBTS2026.Infrastructure.Queries.Users;
using IBTS2026.Infrastructure.Repositories.Incidents;
using IBTS2026.Infrastructure.Repositories.Users;
using Microsoft.Extensions.DependencyInjection;

namespace IBTS2026.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();

        // Register User query handlers
        services.AddScoped<IQueryHandler<GetUserQuery, UserDetailsDto?>, GetUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, PagedResult<UserDto>>, GetUsersQueryHandler>();

        // Register Incident query handlers
        services.AddScoped<IQueryHandler<GetIncidentQuery, IncidentDetailsDto?>, GetIncidentQueryHandler>();
        services.AddScoped<IQueryHandler<GetIncidentsQuery, PagedResult<IncidentDto>>, GetIncidentsQueryHandler>();

        return services;
    }
}
