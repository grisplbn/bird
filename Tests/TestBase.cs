using Tests.Infrastructure.Logging; // <- tu masz LoggingHub

namespace Client.Acceptance.Tests.Configuration;

[Collection("SharedCollectionDefinition")]
public abstract class SharedCollectionTestBase
{
    // globalne włączanie/wyłączanie (możesz zawołać np. w static ctorze)
    protected static void SetGlobalLogging(bool enabled) => LoggingHub.SetGlobal(enabled);

    // per-test: w teście wpisujesz po prostu: enableLogging = true;
    protected bool enableLogging
    {
        set => LoggingHub.SetForCurrentTest(value);
    }

    // automatyczne włączenie na podstawie ENV (opcjonalnie)
    static SharedCollectionTestBase()
    {
        var env = Environment.GetEnvironmentVariable("HTTP_LOG");
        if (string.Equals(env, "1", StringComparison.OrdinalIgnoreCase))
            LoggingHub.SetGlobal(true);
    }
}
