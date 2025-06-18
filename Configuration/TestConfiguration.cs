using Microsoft.Extensions.Configuration;
using System;

namespace Bird.Configuration
{
    /// <summary>
    /// Manages test configuration settings for the application.
    /// This class provides access to configuration values from appsettings.json files and environment variables.
    /// It implements the Singleton pattern to ensure only one instance of configuration is loaded.
    /// </summary>
    public class TestConfiguration
    {
        // Static instance of IConfiguration to store loaded configuration
        private static IConfiguration? _configuration;
        // Lock object for thread-safe initialization
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the configuration instance. If not initialized, loads configuration from:
        /// 1. appsettings.json (required)
        /// 2. appsettings.{environment}.json (optional, based on TEST_ENVIRONMENT variable)
        /// 3. Environment variables
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
                            // Get environment from TEST_ENVIRONMENT variable or default to "Development"
                            var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Development";
                            
                            // Build configuration from multiple sources
                            _configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("Config/appsettings.json", optional: false)
                                .AddJsonFile($"Config/appsettings.{environment}.json", optional: true)
                                .AddEnvironmentVariables()
                                .Build();
                        }
                    }
                }
                return _configuration ?? throw new InvalidOperationException("Configuration was not initialized properly.");
            }
        }

        /// <summary>
        /// Gets the API base URL from configuration.
        /// </summary>
        /// <returns>The API base URL</returns>
        /// <exception cref="InvalidOperationException">Thrown when API base URL is not found in configuration</exception>
        public static string GetApiBaseUrl() => 
            Instance["ApiSettings:BaseUrl"] ?? 
            throw new InvalidOperationException("API base URL not found in configuration.");

        /// <summary>
        /// Gets the database connection string from configuration.
        /// </summary>
        /// <returns>The database connection string</returns>
        /// <exception cref="InvalidOperationException">Thrown when connection string is not found in configuration</exception>
        public static string GetDatabaseConnectionString() => 
            Instance["DatabaseSettings:ConnectionString"] ?? 
            throw new InvalidOperationException("Database connection string not found in configuration.");
    }
} 