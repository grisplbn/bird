using Tests.Infrastructure.Logging;

namespace Client.Acceptance.Tests.Configuration;

[Collection("SharedCollectionDefinition")]
public abstract class SharedCollectionTestBase
{
    static SharedCollectionTestBase()
    {
        // Upewnia się, że Trace trafia do Test Explorera / pliku
        LoggingHub.EnsureTraceListeners();

        // Opcjonalnie: automatyczne globalne włączenie z ENV
        if (string.Equals(Environment.GetEnvironmentVariable("HTTP_LOG"), "1", StringComparison.OrdinalIgnoreCase))
            LoggingHub.SetGlobal(true);
    }

    // Globalne sterowanie (możesz wywołać raz z dowolnego miejsca)
    protected static void SetGlobalLogging(bool enabled) => LoggingHub.SetGlobal(enabled);

    // Per-test: w teście wystarczy napisać:  enableLogging = true;
    protected bool enableLogging
    {
        set => LoggingHub.SetForCurrentTest(value);
    }
}
