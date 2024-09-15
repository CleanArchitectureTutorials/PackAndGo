using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PackAndGo.Domain.Repositories;
using PackAndGo.Infrastructure.Persistence;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using PackAndGo.Application.Interfaces;
using PackAndGo.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace PackAndGo.Infrastructure.Configuration;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind the "ConnectionStrings" section to DatabaseOptions
        services.Configure<DatabaseOptions>(configuration.GetSection("ConnectionStrings"));
        
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            // Retrieve the DefaultConnection string from the configured DatabaseOptions
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlite(databaseOptions.DefaultConnection);
        });

        services.AddIdentityCore<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Add authentication services
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	        .AddCookie();

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, CookieAuthService>();
    }
}
