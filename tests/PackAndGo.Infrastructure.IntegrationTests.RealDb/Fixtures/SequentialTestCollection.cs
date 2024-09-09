using PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures;
using Xunit;

[CollectionDefinition("Sequential Test Collection", DisableParallelization = true)]
public class SequentialTestCollection : ICollectionFixture<DatabaseFixture>
{
}
