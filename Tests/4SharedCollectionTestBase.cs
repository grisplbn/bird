using Client.Acceptance.Tests.Infrastructure.Logging;
using Xunit.Abstractions;

namespace Client.Acceptance.Tests.Configuration;

[Collection("SharedCollectionDefinition")]
public abstract class SharedCollectionTestBase
{
    // Konstruktor, w który xUnit SAM przekaże ITestOutputHelper
    protected SharedCollectionTestBase(ITestOutputHelper output)
    {
        RequestLog.UseITestOutput(output);  // logi do Output (Tests)
        RequestLog.SetGlobal(true);         // globalnie ON
    }

    // (opcjonalnie) per-test
    protected bool enableLogging
    {
        set => RequestLog.SetForCurrentTest(value);
    }
}
