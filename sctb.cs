using Client.Acceptance.Tests.Infrastructure.Logging;

namespace Client.Acceptance.Tests.Configuration;

[Collection("SharedCollectionDefinition")]
public abstract class SharedCollectionTestBase
{
    static SharedCollectionTestBase()
    {
        // globalnie: włącz logowanie i fallback na konsolę (Test Explorer zbierze stdout)
        OutputHub.SetGlobal(true);
        OutputHub.EnableConsoleFallback(true);
    }

    // jeśli chcesz w czasie testów globalnie wyłączyć/włączyć:
    protected static void SetGlobalLogging(bool enabled) => OutputHub.SetGlobal(enabled);

    // per-test: w treści testu możesz napisać enableLogging = true/false
    protected bool enableLogging
    {
        set => OutputHub.SetForCurrentTest(value);
    }
}
