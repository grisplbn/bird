using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using Bird.Framework;
using NUnit.Framework;

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
        public async Task CreateUser_ShouldReturnCreatedUserWithId()
        {
            // Arrange
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "John Doe" },
                    { "email", "john.doe@example.com" }
                });

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.Created);
            
            // Extract and store the user ID for later use
            var userId = await ExtractValueFromResponseAsync<string>(response, "data.id");
            Assert.That(userId, Is.Not.Empty, "User ID should not be empty");

            // Assert other fields in the response
            await AssertResponseFieldAsync(response, "data.name", "John Doe");
            await AssertResponseFieldAsync(response, "data.email", "john.doe@example.com");
            await AssertResponseFieldAsync(response, "data.status", "active");
        }

        /// <summary>
        /// Tests creating a user with invalid data.
        /// Demonstrates how to:
        /// - Test error scenarios
        /// - Verify error responses
        /// - Extract error messages
        /// </summary>
        [Test]
        public async Task CreateUser_WithInvalidData_ShouldReturnValidationErrors()
        {
            // Arrange
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "" },  // Invalid empty name
                    { "email", "invalid-email" }  // Invalid email format
                });

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.BadRequest);
            
            // Extract and assert validation errors
            var errors = await ExtractValueFromResponseAsync<Dictionary<string, string[]>>(response, "errors");
            Assert.That(errors.ContainsKey("name"), "Should have name validation error");
            Assert.That(errors.ContainsKey("email"), "Should have email validation error");
            
            // Assert specific error messages
            await AssertResponseFieldAsync(response, "errors.name[0]", "Name is required");
            await AssertResponseFieldAsync(response, "errors.email[0]", "Invalid email format");
        }

        [Test]
        public async Task GetUser_ShouldReturnUserDetails()
        {
            // Arrange
            var createPayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "Jane Smith" },
                    { "email", "jane.smith@example.com" }
                });

            // Create a user first
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", createPayload);
            var userId = await ExtractValueFromResponseAsync<string>(createResponse, "data.id");

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Get, $"users/{userId}");

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.OK);
            await AssertResponseFieldAsync(response, "data.id", userId);
            await AssertResponseFieldAsync(response, "data.name", "Jane Smith");
            await AssertResponseFieldAsync(response, "data.email", "jane.smith@example.com");
        }

        [Test]
        public async Task UpdateUser_ShouldModifyUserDetails()
        {
            // Arrange
            var createPayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "Original Name" },
                    { "email", "original@example.com" }
                });

            // Create a user first
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", createPayload);
            var userId = await ExtractValueFromResponseAsync<string>(createResponse, "data.id");

            // Prepare update payload
            var updatePayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "update-user.json",
                new Dictionary<string, object>
                {
                    { "name", "Updated Name" },
                    { "email", "updated@example.com" }
                });

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Put, $"users/{userId}", updatePayload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.OK);
            await AssertResponseFieldAsync(response, "data.id", userId);
            await AssertResponseFieldAsync(response, "data.name", "Updated Name");
            await AssertResponseFieldAsync(response, "data.email", "updated@example.com");
        }

        [Test]
        public async Task DeleteUser_ShouldRemoveUser()
        {
            // Arrange
            var createPayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "To Be Deleted" },
                    { "email", "delete@example.com" }
                });

            // Create a user first
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", createPayload);
            var userId = await ExtractValueFromResponseAsync<string>(createResponse, "data.id");

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Delete, $"users/{userId}");

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.NoContent);

            // Verify user is deleted by trying to get it
            var getResponse = await SendRequestAsync<JsonNode>(HttpMethod.Get, $"users/{userId}");
            AssertResponseCode(getResponse, (int)HttpStatusCode.NotFound);
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