using System.Net;
using System.Text.Json.Nodes;
using Bird.Framework;

namespace Bird.Tests
{
    /// <summary>
    /// Sample API tests demonstrating how to use the test framework.
    /// These tests show examples of:
    /// - Making API requests
    /// - Handling responses
    /// - Working with JSON payloads
    /// - Asserting results
    /// </summary>
    [TestFixture]
    public class SampleApiTests : BaseApiTest
    {
        private JsonPayloadManager _payloadManager = null!;

        /// <summary>
        /// Sets up the test environment before each test.
        /// Initializes the JsonPayloadManager for handling test data.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _payloadManager = new JsonPayloadManager();
        }

        /// <summary>
        /// Tests the health check endpoint.
        /// Verifies that the API is running and responding correctly.
        /// </summary>
        [Test]
        public async Task HealthCheck_ShouldReturnOk()
        {
            // Act
            var response = await SendRequestAsync<object>(HttpMethod.Get, "health");

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Tests creating a new user.
        /// Demonstrates how to:
        /// - Load and modify a JSON payload
        /// - Make a POST request
        /// - Extract and verify the response
        /// </summary>
        [Test]
        public async Task CreateUser_ShouldReturnCreated()
        {
            // Arrange
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "email", $"test_{Guid.NewGuid()}@example.com" }
                }
            );

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.Created);
            var userId = await ExtractGuidFromResponseAsync(response);
            Assert.That(!string.IsNullOrEmpty(userId), "User ID should not be empty");
        }

        /// <summary>
        /// Tests creating a user with invalid data.
        /// Demonstrates how to:
        /// - Test error scenarios
        /// - Verify error responses
        /// - Extract error messages
        /// </summary>
        [Test]
        public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "email", "invalid-email" }
                }
            );

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.BadRequest);
            var error = await ExtractErrorFromResponseAsync(response);
            Assert.That(error, Is.Not.Empty, "Error message should not be empty");
        }

        [Test]
        public async Task CreateResource_Success()
        {
            // Arrange
            var modifications = new Dictionary<string, object>
            {
                { "name", "Test Resource" },
                { "description", "This is a test resource" }
            };

            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-resource.json",
                modifications);

            // Act
            var response = await SendRequestAsync(
                HttpMethod.Post,
                "/api/resources",
                payload);

            // Assert
            AssertResponseCode(response, 201);
            var guid = await ExtractGuidFromResponseAsync(response);
            Assert.That(guid, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task CreateResource_WithMissingRequiredField_ReturnsBadRequest()
        {
            // Arrange
            var fieldsToRemove = new List<string> { "name" };
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-resource.json",
                fieldsToRemove: fieldsToRemove);

            // Act
            var response = await SendRequestAsync(
                HttpMethod.Post,
                "/api/resources",
                payload);

            // Assert
            AssertResponseCode(response, 400);
            var error = await ExtractErrorFromResponseAsync(response);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task CreateResource_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var modifications = new Dictionary<string, object>
            {
                { "name", "" }, // Empty name should be invalid
                { "description", "This is a test resource" }
            };

            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-resource.json",
                modifications);

            // Act
            var response = await SendRequestAsync(
                HttpMethod.Post,
                "/api/resources",
                payload);

            // Assert
            AssertResponseCode(response, 400);
            var error = await ExtractErrorFromResponseAsync(response);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
        }
    }
} 