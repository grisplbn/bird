using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;
using Bird.Configuration;

namespace Bird.Framework
{
    /// <summary>
    /// Base class for all API tests. Provides common functionality for making HTTP requests,
    /// handling responses, and asserting results. This class should be inherited by all API test classes.
    /// </summary>
    public abstract class BaseApiTest
    {
        // HttpClient instance for making API requests
        protected HttpClient _httpClient = null!;
        // JSON serialization options
        protected JsonSerializerOptions _jsonOptions = null!;
        // Logger instance
        protected TestLogger _logger = null!;

        /// <summary>
        /// Sets up the test environment before running the test.
        /// </summary>
        /// <param name="environment">Optional environment name. If not provided, uses the environment from command line.</param>
        protected void SetupTestEnvironment(string? environment = null)
        {
            if (!string.IsNullOrEmpty(environment))
            {
                TestConfiguration.SetTestEnvironment(environment);
            }
        }

        /// <summary>
        /// Cleans up the test environment after running the test.
        /// </summary>
        protected void CleanupTestEnvironment()
        {
            TestConfiguration.ResetTestEnvironment();
        }

        /// <summary>
        /// One-time setup for all tests in the fixture.
        /// Initializes the HTTP client and JSON options.
        /// </summary>
        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(TestConfiguration.GetApiBaseUrl())
            };
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Setup method that runs before each test.
        /// Initializes the logger for the current test.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            _logger = new TestLogger(TestContext.CurrentContext.Test.Name);
            _logger.LogInfo($"Starting test: {TestContext.CurrentContext.Test.Name}");
        }

        /// <summary>
        /// Teardown method that runs after each test.
        /// Saves the test log and cleans up resources.
        /// </summary>
        [TearDown]
        public virtual async Task TearDown()
        {
            if (_logger != null)
            {
                _logger.LogInfo($"Finished test: {TestContext.CurrentContext.Test.Name}");
                await _logger.SaveLogAsync();
            }
        }

        /// <summary>
        /// One-time cleanup for all tests in the fixture.
        /// Disposes of the HTTP client.
        /// </summary>
        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            _httpClient?.Dispose();
        }

        /// <summary>
        /// Sends an HTTP request to the specified endpoint.
        /// </summary>
        /// <typeparam name="T">Type of the payload</typeparam>
        /// <param name="method">HTTP method (GET, POST, etc.)</param>
        /// <param name="endpoint">API endpoint to call</param>
        /// <param name="payload">Request payload (optional)</param>
        /// <param name="headers">Additional HTTP headers (optional)</param>
        /// <returns>HTTP response message</returns>
        protected async Task<HttpResponseMessage> SendRequestAsync<T>(
            HttpMethod method,
            string endpoint,
            T? payload = default,
            Dictionary<string, string>? headers = null)
        {
            _logger.LogRequest(method.ToString(), endpoint, payload);

            var request = new HttpRequestMessage(method, endpoint);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (payload != null)
            {
                request.Content = JsonContent.Create(payload, options: _jsonOptions);
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogResponse((int)response.StatusCode, content);
            
            return response;
        }

        /// <summary>
        /// Deserializes the response content to the specified type.
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="response">HTTP response message</param>
        /// <returns>Deserialized object</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails</exception>
        protected async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                
                if (result == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to deserialize response to {typeof(T).Name}", ex);
                throw;
            }
        }

        /// <summary>
        /// Asserts that the response has the expected HTTP status code.
        /// </summary>
        /// <param name="response">HTTP response message</param>
        /// <param name="expectedCode">Expected HTTP status code</param>
        protected void AssertResponseCode(HttpResponseMessage response, int expectedCode)
        {
            var actualCode = (int)response.StatusCode;
            _logger.LogInfo($"Asserting response code: Expected {expectedCode}, Got {actualCode}");
            
            Assert.That(actualCode, Is.EqualTo(expectedCode),
                $"Expected status code {expectedCode} but got {actualCode}");
        }

        /// <summary>
        /// Extracts a GUID from the response JSON.
        /// </summary>
        /// <param name="response">HTTP response message</param>
        /// <returns>GUID as string</returns>
        /// <exception cref="InvalidOperationException">Thrown when GUID cannot be extracted</exception>
        protected async Task<string> ExtractGuidFromResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            return jsonDoc.RootElement.GetProperty("id").GetString() ?? 
                throw new InvalidOperationException("Failed to extract GUID from response");
        }

        /// <summary>
        /// Extracts an error message from the response JSON.
        /// </summary>
        /// <param name="response">HTTP response message</param>
        /// <returns>Error message</returns>
        /// <exception cref="InvalidOperationException">Thrown when error message cannot be extracted</exception>
        protected async Task<string> ExtractErrorFromResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            return jsonDoc.RootElement.GetProperty("error").GetString() ?? 
                throw new InvalidOperationException("Failed to extract error message from response");
        }

        /// <summary>
        /// Extracts a value from the response JSON using a JSON path.
        /// </summary>
        /// <typeparam name="T">Type of the value to extract</typeparam>
        /// <param name="response">HTTP response message</param>
        /// <param name="jsonPath">JSON path to the value (e.g., "data.id" or "user.name")</param>
        /// <returns>Extracted value</returns>
        /// <exception cref="InvalidOperationException">Thrown when value cannot be extracted</exception>
        protected async Task<T> ExtractValueFromResponseAsync<T>(HttpResponseMessage response, string jsonPath)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var pathParts = jsonPath.Split('.');
                var currentElement = jsonDoc.RootElement;

                foreach (var part in pathParts)
                {
                    if (currentElement.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidOperationException($"Cannot navigate to '{part}' in path '{jsonPath}'");
                    }
                    currentElement = currentElement.GetProperty(part);
                }

                var value = currentElement.Deserialize<T>(_jsonOptions);
                if (value == null)
                {
                    throw new InvalidOperationException($"Failed to extract value at path '{jsonPath}'");
                }

                _logger.LogInfo($"Extracted value from path '{jsonPath}': {value}");
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract value from path '{jsonPath}'", ex);
                throw;
            }
        }

        /// <summary>
        /// Asserts that a specific field in the response matches the expected value.
        /// </summary>
        /// <typeparam name="T">Type of the value to compare</typeparam>
        /// <param name="response">HTTP response message</param>
        /// <param name="jsonPath">JSON path to the value (e.g., "data.id" or "user.name")</param>
        /// <param name="expectedValue">Expected value</param>
        protected async Task AssertResponseFieldAsync<T>(HttpResponseMessage response, string jsonPath, T expectedValue)
        {
            try
            {
                var actualValue = await ExtractValueFromResponseAsync<T>(response, jsonPath);
                _logger.LogInfo($"Asserting field '{jsonPath}': Expected {expectedValue}, Got {actualValue}");
                
                Assert.That(actualValue, Is.EqualTo(expectedValue),
                    $"Expected value at path '{jsonPath}' to be {expectedValue} but got {actualValue}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to assert field '{jsonPath}'", ex);
                throw;
            }
        }
    }
} 