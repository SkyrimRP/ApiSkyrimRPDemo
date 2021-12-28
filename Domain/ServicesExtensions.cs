using Domain.Services.JwtAuthManager;
using Domain.Services.JwtAuthManager.Abstractions;
using Domain.Services.Players;
using Domain.Services.Players.Abstractions;
using Domain.Services.Servers;
using Domain.Services.Servers.Abstarctions;
using Domain.Services.Users;
using Domain.Services.Users.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class ServicesExtensions
    {
        public static void AddDomain(this IServiceCollection service)
        {
            service.AddScoped<IUsersService, UsersService>();
            service.AddScoped<IServersService, ServersService>();
            service.AddScoped<IPlayersService, PlayersService>();

            service.AddSingleton<IJwtAuthManager, JwtAuthManager>();
            service.AddHostedService<JwtRefreshTokenCache>();

            //service.AddTransient<DataInitilaizer>();
        }
    }
}
