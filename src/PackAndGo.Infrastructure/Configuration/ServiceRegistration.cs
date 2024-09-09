using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PackAndGo.Domain.Repositories;
using PackAndGo.Infrastructure.Persistence;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.Options;

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

        services.AddScoped<IUserRepository, UserRepository>();
    }
}