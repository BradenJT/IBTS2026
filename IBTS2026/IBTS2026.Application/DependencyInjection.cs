using FluentValidation;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Features.Users.CreateUser;
using IBTS2026.Application.Features.Users.RemoveUser;
using IBTS2026.Application.Features.Users.UpdateUser;
using Microsoft.Extensions.DependencyInjection;

namespace IBTS2026.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register the request dispatcher
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();

        // Register command handlers
        services.AddScoped<IRequestHandler<CreateUserCommand, int>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, bool>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<RemoveUserCommand, bool>, RemoveUserHandler>();

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();

        return services;
    }
}
