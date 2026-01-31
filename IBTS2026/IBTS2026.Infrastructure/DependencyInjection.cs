using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Dtos.IncidentNotes;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.IncidentNotes.GetIncidentNotes;
using IBTS2026.Application.Features.Incidents.GetIncident;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Domain.Interfaces.IncidentNotes;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Domain.Interfaces.Notifications;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using IBTS2026.Infrastructure.Queries.IncidentNotes;
using IBTS2026.Infrastructure.Queries.Incidents;
using IBTS2026.Infrastructure.Queries.Users;
using IBTS2026.Infrastructure.Repositories.IncidentNotes;
using IBTS2026.Infrastructure.Repositories.Incidents;
using IBTS2026.Infrastructure.Repositories.Notifications;
using IBTS2026.Infrastructure.Repositories.Users;
using IBTS2026.Infrastructure.Services;
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
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IIncidentNoteRepository, IncidentNoteRepository>();
        services.AddScoped<INotificationOutboxRepository, NotificationOutboxRepository>();

        // Register services
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Register User query handlers
        services.AddScoped<IRequestHandler<GetUserQuery, UserDetailsDto?>, GetUserQueryHandler>();
        services.AddScoped<IRequestHandler<GetUsersQuery, PagedResult<UserDto>>, GetUsersQueryHandler>();

        // Register Incident query handlers
        services.AddScoped<IRequestHandler<GetIncidentQuery, IncidentDetailsDto?>, GetIncidentQueryHandler>();
        services.AddScoped<IRequestHandler<GetIncidentsQuery, PagedResult<IncidentDto>>, GetIncidentsQueryHandler>();

        // Register IncidentNote query handlers
        services.AddScoped<IRequestHandler<GetIncidentNotesQuery, IReadOnlyList<IncidentNoteDto>>, GetIncidentNotesQueryHandler>();

        return services;
    }
}
