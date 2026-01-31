using FluentValidation;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Features.Auth.InviteUser;
using IBTS2026.Application.Features.Auth.Login;
using IBTS2026.Application.Features.Auth.RegisterUser;
using IBTS2026.Application.Features.Incidents.CreateIncident;
using IBTS2026.Application.Features.Incidents.RemoveIncident;
using IBTS2026.Application.Features.Incidents.UpdateIncident;
using IBTS2026.Application.Features.IncidentNotes.CreateIncidentNote;
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

        // Register Auth command handlers
        services.AddScoped<IRequestHandler<RegisterUserCommand, RegisterUserResult>, RegisterUserHandler>();
        services.AddScoped<IRequestHandler<LoginCommand, LoginResult>, LoginHandler>();
        services.AddScoped<IRequestHandler<InviteUserCommand, InviteUserResult>, InviteUserHandler>();

        // Register User command handlers
        services.AddScoped<IRequestHandler<CreateUserCommand, int>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, bool>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<RemoveUserCommand, bool>, RemoveUserHandler>();

        // Register Incident command handlers
        services.AddScoped<IRequestHandler<CreateIncidentCommand, int>, CreateIncidentHandler>();
        services.AddScoped<IRequestHandler<UpdateIncidentCommand, bool>, UpdateIncidentHandler>();
        services.AddScoped<IRequestHandler<RemoveIncidentCommand, bool>, RemoveIncidentHandler>();

        // Register IncidentNote command handlers
        services.AddScoped<IRequestHandler<CreateIncidentNoteCommand, int>, CreateIncidentNoteHandler>();

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();

        return services;
    }
}
