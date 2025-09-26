using Tests.Infrastructure.Logging;

namespace Tests.Infrastructure;

public abstract class TestBase
{
    /// <summary>
    /// Globalne włączanie/wyłączanie logów dla całego runa.
    /// Wywołaj JEDEN raz np. w statycznym konstruktorze swojej bazy,
    /// albo ustaw zmienną środowiskową HTTP_LOG=1.
    /// </summary>
    protected static void SetGlobalLogging(bool enabled) => LoggingHub.SetGlobal(enabled);

    /// <summary>
    /// Flaga per-test. W teście wystarczy wpisać: enableLogging = true;
    /// Jeśli nie ruszasz – nic się nie odpala.
    /// </summary>
    protected bool enableLogging
    {
        set => LoggingHub.SetForCurrentTest(value);
    }

    // (opcjonalnie) możesz w statycznym ctorze wymusić globalne logi, np. z configu:
    // static TestBase() => SetGlobalLogging(true);
}
