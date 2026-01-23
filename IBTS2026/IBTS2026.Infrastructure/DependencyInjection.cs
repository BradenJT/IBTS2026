using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Infrastructure.Queries.Users;
using IBTS2026.Infrastructure.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IBTS2026.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<IBTS2026Context>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("IBTS2026"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetUserQuery, UserDetailsDto?>, GetUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, PagedResult<UserDto>>, GetUsersQueryHandler>();

        return services;
    }
}
