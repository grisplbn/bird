using Microsoft.Extensions.Configuration;
using System;

namespace Bird.Configuration
{
    /// <summary>
    /// Manages test configuration settings. Implements the Singleton pattern to ensure
    /// configuration is loaded only once and shared across all tests.
    /// </summary>
    public class TestConfiguration
    {
        private static IConfiguration? _configuration;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of the configuration.
        /// </summary>
        public static IConfiguration Instance
        {
            get
            {
                if (_configuration == null)
                {
                    lock (_lock)
                    {
                        if (_configuration == null)
                        {
                            var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Development";
                            var builder = new ConfigurationBuilder()
                                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"Config/appsettings.{environment}.json", optional: true, reloadOnChange: true)
                                .AddEnvironmentVariables();

                            _configuration = builder.Build();
                        }
                    }
                }
                return _configuration;
            }
        }

        /// <summary>
        /// Gets the API base URL from configuration.
        /// </summary>
        /// <returns>The API base URL</returns>
        /// <exception cref="InvalidOperationException">Thrown when the API base URL is not found in configuration</exception>
        public static string GetApiBaseUrl()
        {
            var baseUrl = Instance["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("API base URL not found in configuration");
            }
            return baseUrl;
        }

        /// <summary>
        /// Gets the database connection string from configuration.
        /// </summary>
        /// <returns>The database connection string</returns>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is not found in configuration</exception>
        public static string GetDatabaseConnectionString()
        {
            var connectionString = Instance["DatabaseSettings:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string not found in configuration");
            }
            return connectionString;
        }
    }
} 