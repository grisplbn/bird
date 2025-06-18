using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bird.Framework
{
    /// <summary>
    /// Provides logging functionality for test execution.
    /// </summary>
    public class TestLogger
    {
        private readonly string _logDirectory;
        private readonly string _testName;
        private readonly StringBuilder _logBuilder;
        private readonly object _lockObject = new object();

        // Console colors for different log levels
        private static class ConsoleColors
        {
            public static ConsoleColor Info = ConsoleColor.White;
            public static ConsoleColor Warn = ConsoleColor.Yellow;
            public static ConsoleColor Error = ConsoleColor.Red;
            public static ConsoleColor Request = ConsoleColor.Cyan;
            public static ConsoleColor Response = ConsoleColor.Green;
            public static ConsoleColor Timestamp = ConsoleColor.Gray;
        }

        public TestLogger(string testName)
        {
            _testName = testName;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logBuilder = new StringBuilder();
            
            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public void LogInfo(string message)
        {
            Log("INFO", message, ConsoleColors.Info);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void LogWarning(string message)
        {
            Log("WARN", message, ConsoleColors.Warn);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public void LogError(string message, Exception? exception = null)
        {
            Log("ERROR", message, ConsoleColors.Error);
            if (exception != null)
            {
                Log("ERROR", $"Exception: {exception.Message}", ConsoleColors.Error);
                Log("ERROR", $"Stack Trace: {exception.StackTrace}", ConsoleColors.Error);
            }
        }

        /// <summary>
        /// Logs a request to the API.
        /// </summary>
        public void LogRequest(string method, string endpoint, object? payload = null)
        {
            Log("REQUEST", $"{method} {endpoint}", ConsoleColors.Request);
            if (payload != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(payload, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Log("REQUEST", $"Payload:\n{json}", ConsoleColors.Request);
            }
        }

        /// <summary>
        /// Logs a response from the API.
        /// </summary>
        public void LogResponse(int statusCode, string content)
        {
            var statusColor = statusCode >= 400 ? ConsoleColors.Error : ConsoleColors.Response;
            Log("RESPONSE", $"Status Code: {statusCode}", statusColor);
            
            try
            {
                // Try to format JSON for better readability
                var json = System.Text.Json.JsonSerializer.Serialize(
                    System.Text.Json.JsonSerializer.Deserialize<object>(content),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                );
                Log("RESPONSE", $"Content:\n{json}", statusColor);
            }
            catch
            {
                // If not JSON, just log the content as is
                Log("RESPONSE", $"Content: {content}", statusColor);
            }
        }

        /// <summary>
        /// Logs a message with the specified level.
        /// </summary>
        private void Log(string level, string message, ConsoleColor levelColor)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{level}] {message}";
            
            lock (_lockObject)
            {
                _logBuilder.AppendLine(logMessage);

                // Write to console with colors
                var originalColor = Console.ForegroundColor;
                try
                {
                    // Write timestamp
                    Console.ForegroundColor = ConsoleColors.Timestamp;
                    Console.Write($"[{timestamp}] ");

                    // Write level
                    Console.ForegroundColor = levelColor;
                    Console.Write($"[{level}] ");

                    // Write message
                    Console.ForegroundColor = levelColor;
                    Console.WriteLine(message);
                }
                finally
                {
                    Console.ForegroundColor = originalColor;
                }
            }
        }

        /// <summary>
        /// Saves the log to a file.
        /// </summary>
        public async Task SaveLogAsync()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{_testName}_{timestamp}.log";
            var filePath = Path.Combine(_logDirectory, fileName);

            lock (_lockObject)
            {
                File.WriteAllText(filePath, _logBuilder.ToString());
            }
        }

        /// <summary>
        /// Clears the current log buffer.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _logBuilder.Clear();
            }
        }
    }
} 