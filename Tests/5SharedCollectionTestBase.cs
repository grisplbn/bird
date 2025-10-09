using Client.Acceptance.Tests.Infrastructure.Logging;

namespace Client.Acceptance.Tests.Configuration;

[Collection("SharedCollectionDefinition")]
public abstract class SharedCollectionTestBase
{
    protected bool enableLogging
    {
        set => RequestLog.SetForCurrentTest(value);
    }
}
