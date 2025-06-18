using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using Allure.NUnit.Attributes;
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
    /// - Environment selection
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
        /// Tests the health check endpoint using the default environment.
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
        /// Tests the health check endpoint using a specific environment.
        /// </summary>
        [Test]
        public async Task HealthCheck_ShouldReturnOk_OnStaging()
        {
            // Arrange
            SetupTestEnvironment("Staging");

            try
            {
                // Act
                var response = await SendRequestAsync<object>(HttpMethod.Get, "health");

                // Assert
                AssertResponseCode(response, (int)HttpStatusCode.OK);
            }
            finally
            {
                // Cleanup
                CleanupTestEnvironment();
            }
        }

        /// <summary>
        /// Tests creating a new user.
        /// Demonstrates how to:
        /// - Load and modify a JSON payload
        /// - Make a POST request
        /// - Extract and verify the response
        /// </summary>
        [Test]
        [AllureIssue("API-123")]
        [AllureIssue("API-124")]
        [AllureTms("API-125")]
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
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, ApiEndpoints.Users.Create, payload);

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
        [AllureIssue("API-126")]
        [AllureTms("API-127")]
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
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, ApiEndpoints.Users.Create, payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.BadRequest);
            
            // Extract and assert validation errors
            await AssertResponseFieldAsync(response, "errors.name", "Name is required");
            await AssertResponseFieldAsync(response, "errors.email", "Invalid email format");
        }

        [Test]
        [AllureIssue("API-128")]
        [AllureTms("API-129")]
        public async Task GetUser_ShouldReturnUserDetails()
        {
            // Arrange
            var createPayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "Jane Doe" },
                    { "email", "jane.doe@example.com" }
                });

            // Create a user first
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, ApiEndpoints.Users.Create, createPayload);
            var userId = await ExtractValueFromResponseAsync<string>(createResponse, "data.id");

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Get, ApiEndpoints.Users.GetById(userId));

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.OK);
            await AssertResponseFieldAsync(response, "data.id", userId);
            await AssertResponseFieldAsync(response, "data.name", "Jane Doe");
            await AssertResponseFieldAsync(response, "data.email", "jane.doe@example.com");
        }

        [Test]
        [AllureIssue("API-130")]
        [AllureTms("API-131")]
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
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, ApiEndpoints.Users.Create, createPayload);
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
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Put, ApiEndpoints.Users.Update(userId), updatePayload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.OK);
            await AssertResponseFieldAsync(response, "data.id", userId);
            await AssertResponseFieldAsync(response, "data.name", "Updated Name");
            await AssertResponseFieldAsync(response, "data.email", "updated@example.com");
        }

        [Test]
        [AllureIssue("API-132")]
        [AllureTms("API-133")]
        public async Task DeleteUser_ShouldRemoveUser()
        {
            // Arrange
            var createPayload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-user.json",
                new Dictionary<string, object>
                {
                    { "name", "Delete Me" },
                    { "email", "delete.me@example.com" }
                });

            // Create a user first
            var createResponse = await SendRequestAsync<JsonNode>(HttpMethod.Post, ApiEndpoints.Users.Create, createPayload);
            var userId = await ExtractValueFromResponseAsync<string>(createResponse, "data.id");

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Delete, ApiEndpoints.Users.Delete(userId));

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.NoContent);

            // Verify user is deleted by trying to get it
            var getResponse = await SendRequestAsync<JsonNode>(HttpMethod.Get, ApiEndpoints.Users.GetById(userId));
            AssertResponseCode(getResponse, (int)HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Tests creating a resource using the default environment.
        /// </summary>
        [Test]
        [AllureIssue("API-123")]
        [AllureTms("API-125")]
        public async Task CreateResource_ShouldReturnCreatedResource()
        {
            // Arrange
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-resource.json",
                new Dictionary<string, object>
                {
                    { "name", "Test Resource" },
                    { "description", "This is a test resource" }
                });

            // Act
            var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "resources", payload);

            // Assert
            AssertResponseCode(response, (int)HttpStatusCode.Created);
            await AssertResponseFieldAsync(response, "data.name", "Test Resource");
        }

        /// <summary>
        /// Tests creating a resource using a specific environment.
        /// </summary>
        [Test]
        [AllureIssue("API-124")]
        [AllureTms("API-126")]
        public async Task CreateResource_ShouldReturnCreatedResource_OnStaging()
        {
            // Arrange
            SetupTestEnvironment("Staging");
            var payload = await _payloadManager.LoadAndModifyPayloadAsync(
                "create-resource.json",
                new Dictionary<string, object>
                {
                    { "name", "Test Resource" },
                    { "description", "This is a test resource" }
                });

            try
            {
                // Act
                var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "resources", payload);

                // Assert
                AssertResponseCode(response, (int)HttpStatusCode.Created);
                await AssertResponseFieldAsync(response, "data.name", "Test Resource");
            }
            finally
            {
                // Cleanup
                CleanupTestEnvironment();
            }
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