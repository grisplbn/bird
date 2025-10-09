using Client.Acceptance.Tests.Infrastructure.Logging;
using Xunit.Abstractions;

namespace Client.Acceptance.Tests.Configuration;

// TEN fixture zostanie uruchomiony dla całej kolekcji (bez zmian w testach)
public sealed class LoggingCollectionFixture
{
    public LoggingCollectionFixture(IMessageSink diagnosticSink)
    {
        // kieruj logi do xUnit Diagnostic sink (Output → Tests)
        RequestLog.UseXunitDiagnosticSink(diagnosticSink);

        // możesz też globalnie włączyć/wyłączyć logowanie
        RequestLog.SetGlobal(true);
    }
}
