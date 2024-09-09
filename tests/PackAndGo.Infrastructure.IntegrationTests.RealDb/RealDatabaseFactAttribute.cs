using Xunit;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb;

public class RealDatabaseFactAttribute : FactAttribute
{
    public RealDatabaseFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USE_REAL_DATABASE")))
        {
            Skip = "Skipping test because USE_REAL_DATABASE is not set.";
        }
    }
}
