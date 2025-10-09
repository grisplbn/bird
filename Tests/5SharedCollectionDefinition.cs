using Xunit;

namespace Client.Acceptance.Tests.Configuration;

[CollectionDefinition("SharedCollectionDefinition")]
public class SharedCollectionDefinition
    : ICollectionFixture<ApplicationFactory>,
      ICollectionFixture<LoggingCollectionFixture>   // ⟵ DODANE
{
}
