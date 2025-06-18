# Bird Test Framework

A robust test framework for API testing, built with .NET 8.0 and NUnit. This framework provides a structured approach to API testing with support for different environments and configurations.

## Project Structure

```
Bird/
├── Config/                    # Configuration files
│   ├── appsettings.json      # Base configuration
│   ├── appsettings.Development.json
│   ├── appsettings.Staging.json
│   └── appsettings.Production.json
├── Configuration/            # Configuration management
│   └── TestConfiguration.cs  # Configuration loader
├── Framework/               # Core framework components
│   ├── BaseApiTest.cs      # Base test class
│   └── JsonPayloadManager.cs # JSON payload handling
├── TestData/               # Test data files
│   └── create-user.json    # Sample test data
└── Tests/                  # Test classes
    └── SampleApiTests.cs   # Sample test implementation
```

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Git

## Getting Started

1. Clone the repository:
```bash
git clone <repository-url>
cd bird
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

## Configuration

### Environment-Specific Settings

The framework supports multiple environments through configuration files:

- `Config/appsettings.json`: Base configuration
- `Config/appsettings.Development.json`: Development environment settings
- `Config/appsettings.Staging.json`: Staging environment settings
- `Config/appsettings.Production.json`: Production environment settings

Each environment file can override settings from the base configuration. For example:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.example.com",
    "Timeout": 30
  },
  "DatabaseSettings": {
    "ConnectionString": "your-connection-string"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Running Tests in Different Environments

#### Using Environment Variables

```powershell
# Development environment
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet test

# Staging environment
$env:ASPNETCORE_ENVIRONMENT="Staging"; dotnet test

# Production environment
$env:ASPNETCORE_ENVIRONMENT="Production"; dotnet test
```

#### Using Command Line Parameters

```powershell
# Development environment
dotnet test --environment Development

# Staging environment
dotnet test --environment Staging

# Production environment
dotnet test --environment Production
```

#### Programmatically in Test Code

```csharp
// At the start of your test class or test setup
TestConfiguration.Environment = "Staging";
```

## Writing Tests

### Base Test Class

All API tests should inherit from `BaseApiTest`:

```csharp
public class YourApiTests : BaseApiTest
{
    [Test]
    public async Task YourTest_ShouldDoSomething()
    {
        // Arrange
        var payload = await _payloadManager.LoadAndModifyPayloadAsync("create-user.json");

        // Act
        var response = await SendRequestAsync<JsonNode>(HttpMethod.Post, "users", payload);

        // Assert
        AssertResponseCode(response, (int)HttpStatusCode.Created);
    }
}
```

### JSON Payload Management

Use `JsonPayloadManager` to handle test data:

```csharp
var payloadManager = new JsonPayloadManager("TestData");

// Basic usage - modify values
var payload = await payloadManager.LoadAndModifyPayloadAsync("create-user.json", 
    new Dictionary<string, object>
    {
        { "email", "test@example.com" }
    });

// Advanced usage - modify values and remove fields
var payload = await payloadManager.LoadAndModifyPayloadAsync(
    "create-user.json",
    modifications: new Dictionary<string, object>
    {
        { "email", "test@example.com" }
    },
    fieldsToRemove: new List<string>
    {
        "metadata.lastLogin",  // Remove a nested field
        "isActive"            // Remove a top-level field
    }
);
```

The `JsonPayloadManager` supports:
- Loading JSON files from the TestData directory
- Modifying values using dot notation (e.g., "user.name")
- Removing fields completely from the JSON structure
- Handling nested objects and arrays

### Response Field Extraction and Assertions

The framework provides flexible ways to extract and assert values from response JSON:

```csharp
// Extract a value and store it for later use
var userId = await ExtractValueFromResponseAsync<string>(response, "data.id");
var userName = await ExtractValueFromResponseAsync<string>(response, "user.name");
var age = await ExtractValueFromResponseAsync<int>(response, "user.age");

// Assert a specific field value
await AssertResponseFieldAsync(response, "data.id", "12345");
await AssertResponseFieldAsync(response, "user.name", "John Doe");
await AssertResponseFieldAsync(response, "user.age", 30);

// Extract and use values in subsequent requests
var userId = await ExtractValueFromResponseAsync<string>(response, "data.id");
var updateResponse = await SendRequestAsync<JsonNode>(HttpMethod.Put, $"users/{userId}", updatePayload);
```

The framework supports:
- Extracting values of any type (string, int, bool, etc.)
- Nested JSON paths (e.g., "user.address.city")
- Storing extracted values for later use
- Direct assertions against expected values
- Clear error messages when assertions fail

### Logging

The framework provides comprehensive logging functionality to help with debugging and test execution tracking:

```csharp
// Logs are automatically created for each test
[Test]
public async Task YourTest_ShouldDoSomething()
{
    // Logs are automatically generated for:
    // - Test start and end
    // - API requests and responses
    // - Assertions and validations
    // - Errors and exceptions
}
```

#### Log Output

The framework provides two types of log output:

1. **Terminal Output**
   - Real-time logging during test execution
   - Color-coded messages for better visibility
   - Formatted JSON for better readability
   - Immediate feedback for debugging

   Example terminal output:
   ```
   [2024-03-14 10:15:30.123] [INFO] Starting test: CreateUser_ShouldReturnCreatedUserWithId
   [2024-03-14 10:15:30.234] [REQUEST] POST users
   [2024-03-14 10:15:30.234] [REQUEST] Payload:
   {
     "name": "John Doe",
     "email": "john@example.com"
   }
   [2024-03-14 10:15:30.345] [RESPONSE] Status Code: 201
   [2024-03-14 10:15:30.345] [RESPONSE] Content:
   {
     "data": {
       "id": "123",
       "name": "John Doe"
     }
   }
   ```

2. **Log Files**
   - Persistent logs stored in the `Logs` directory
   - One log file per test execution
   - Named with test name and timestamp
   - Contains all test execution details

#### Log Levels and Colors

The framework uses different colors for different log levels:
- **INFO** (White): General information about test execution
- **WARN** (Yellow): Warning messages for potential issues
- **ERROR** (Red): Error messages and exceptions
- **REQUEST** (Cyan): API request details
- **RESPONSE** (Green): API response details
- **Timestamp** (Gray): Time information

#### Logging Best Practices

1. **Terminal Output**
   - Use terminal output for immediate feedback
   - Watch for color-coded messages
   - Check formatted JSON for readability
   - Monitor test progress in real-time

2. **Log Files**
   - Review log files for detailed analysis
   - Use logs for debugging failed tests
   - Archive logs for historical reference
   - Implement log rotation if needed

3. **Debugging**
   - Use terminal output for quick debugging
   - Check log files for detailed information
   - Look for color-coded error messages
   - Review formatted JSON responses

4. **CI/CD Integration**
   - Terminal output works in CI/CD pipelines
   - Log files are available as artifacts
   - Color coding helps identify issues
   - JSON formatting improves readability

## Best Practices

### Environment-Specific Configuration

1. **Configuration Files**
   - Keep sensitive data in environment-specific files
   - Use environment variables for secrets
   - Never commit production credentials
   - Document all configuration options

2. **Environment Selection**
   - Use Development for local testing
   - Use Staging for pre-production testing
   - Use Production only for production testing
   - Document environment-specific behaviors

### Test Data Management

1. **JSON Files**
   - Store test data in JSON files
   - Use meaningful file names
   - Keep test data minimal and focused
   - Document data structure and requirements

2. **Data Modification**
   - Use `JsonPayloadManager` for data modifications
   - Keep modifications close to the test
   - Document complex data transformations
   - Use constants for common values

3. **Data Cleanup**
   - Clean up test data after tests
   - Use unique identifiers for test data
   - Document cleanup procedures
   - Handle cleanup failures gracefully

### Test Organization

1. **Test Structure**
   - Group related tests in test classes
   - Use descriptive test names
   - Follow the Arrange-Act-Assert pattern
   - Document test prerequisites

2. **Test Dependencies**
   - Minimize test dependencies
   - Use setup and teardown methods
   - Document test dependencies
   - Handle dependency failures

3. **Test Categories**
   - Categorize tests by feature
   - Use test categories for filtering
   - Document test categories
   - Maintain category consistency

### Error Handling

1. **Response Validation**
   - Test both success and failure scenarios
   - Validate error responses
   - Include appropriate assertions
   - Document expected errors

2. **Error Messages**
   - Use clear error messages
   - Include relevant context
   - Document error scenarios
   - Handle unexpected errors

3. **Logging**
   - Log important test steps
   - Include relevant context
   - Document logging requirements
   - Handle logging failures

## Troubleshooting

### Common Issues

1. **Configuration Not Found**
   - Ensure configuration files are in the correct location
   - Check file names match environment names
   - Verify file properties are set to "Copy to Output Directory"
   - Check file permissions

2. **API Connection Issues**
   - Verify API server is running
   - Check API base URL in configuration
   - Ensure network connectivity
   - Check firewall settings

3. **Test Data Issues**
   - Verify JSON file paths
   - Check JSON syntax
   - Ensure required fields are present
   - Validate data types

4. **Response Assertion Issues**
   - Check response structure
   - Verify field paths
   - Ensure correct data types
   - Check for null values

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 