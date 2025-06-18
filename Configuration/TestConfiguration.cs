using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bird.Configuration
{
    /// <summary>
    /// Manages test configuration settings and implements the Singleton pattern.
    /// This class is responsible for loading configuration from various sources:
    /// - appsettings.json (base configuration)
    /// - appsettings.{environment}.json (environment-specific configuration)
    /// - Environment variables (highest priority)
    /// </summary>
    public class TestConfiguration
    {
        private static IConfiguration? _configuration;
        private static readonly object _lock = new object();
        private static string _environment = "Development"; // Default environment

        /// <summary>
        /// Gets the current environment name (Development, Staging, Production, etc.)
        /// </summary>
        public static string Environment
        {
            get => _environment;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Environment name cannot be null or empty", nameof(value));
                _environment = value;
                _configuration = null; // Force reload of configuration
            }
        }

        /// <summary>
        /// Gets the singleton instance of the configuration.
        /// Initializes the configuration if it hasn't been initialized yet.
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
                            var basePath = AppDomain.CurrentDomain.BaseDirectory;
                            var builder = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"Config/appsettings.{Environment}.json", optional: true, reloadOnChange: true)
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
        /// <exception cref="InvalidOperationException">Thrown when the API base URL is not configured</exception>
        public static string GetApiBaseUrl()
        {
            var baseUrl = Instance["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException(
                    $"API base URL not found in configuration. Please check appsettings.json and appsettings.{Environment}.json files.");
            }
            return baseUrl;
        }

        /// <summary>
        /// Gets the database connection string from configuration.
        /// </summary>
        /// <returns>The database connection string</returns>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is not configured</exception>
        public static string GetDatabaseConnectionString()
        {
            var connectionString = Instance["DatabaseSettings:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Database connection string not found in configuration. Please check appsettings.json and appsettings.{Environment}.json files.");
            }
            return connectionString;
        }

        /// <summary>
        /// Gets a configuration value by key.
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <returns>The configuration value, or null if not found</returns>
        public static string? GetValue(string key)
        {
            return Instance[key];
        }

        /// <summary>
        /// Gets a configuration value by key, with a default value if not found.
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if the key is not found</param>
        /// <returns>The configuration value, or the default value if not found</returns>
        public static string GetValue(string key, string defaultValue)
        {
            return Instance[key] ?? defaultValue;
        }
    }
} 