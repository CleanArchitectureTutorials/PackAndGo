using Microsoft.Extensions.DependencyInjection;
using PackAndGo.Application.Interfaces;
using PackAndGo.Application.Services;

namespace PackAndGo.Application.Configuration;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}