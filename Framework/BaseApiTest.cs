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

        /// <summary>
        /// One-time setup method that runs before any tests in the class.
        /// Initializes the HttpClient with the base URL from configuration and sets up JSON options.
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
        /// One-time cleanup method that runs after all tests in the class.
        /// Disposes of the HttpClient to free resources.
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

            return await _httpClient.SendAsync(request);
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
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? 
                throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
        }

        /// <summary>
        /// Asserts that the response has the expected HTTP status code.
        /// </summary>
        /// <param name="response">HTTP response message</param>
        /// <param name="expectedCode">Expected HTTP status code</param>
        protected void AssertResponseCode(HttpResponseMessage response, int expectedCode)
        {
            Assert.That((int)response.StatusCode, Is.EqualTo(expectedCode),
                $"Expected status code {expectedCode} but got {(int)response.StatusCode}");
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
    }
} 