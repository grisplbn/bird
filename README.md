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

The framework supports multiple environments through configuration files:

- `Config/appsettings.json`: Base configuration
- `Config/appsettings.Development.json`: Development environment settings
- `Config/appsettings.Staging.json`: Staging environment settings
- `Config/appsettings.Production.json`: Production environment settings

### Configuration Structure

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

## Running Tests

### Using Environment Variables

```powershell
# Development environment
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet test

# Staging environment
$env:ASPNETCORE_ENVIRONMENT="Staging"; dotnet test

# Production environment
$env:ASPNETCORE_ENVIRONMENT="Production"; dotnet test
```

### Using Command Line Parameters

```powershell
# Development environment
dotnet test --environment Development

# Staging environment
dotnet test --environment Staging

# Production environment
dotnet test --environment Production
```

### Programmatically in Test Code

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
        var response = await SendRequestAsync(HttpMethod.Post, "users", payload);

        // Assert
        AssertResponseCode(response, (int)HttpStatusCode.Created);  // 201
        // or
        AssertResponseCode(response, 201);  // Using numeric code
    }
}
```

#### HTTP Response Code Assertions

The framework provides flexible ways to assert HTTP response codes:

```csharp
// Using HttpStatusCode enum
AssertResponseCode(response, (int)HttpStatusCode.OK);        // 200
AssertResponseCode(response, (int)HttpStatusCode.Created);   // 201
AssertResponseCode(response, (int)HttpStatusCode.BadRequest);// 400

// Using numeric codes
AssertResponseCode(response, 200);  // OK
AssertResponseCode(response, 201);  // Created
AssertResponseCode(response, 400);  // Bad Request
```

The assertion provides clear error messages when the expected code doesn't match:
```
Expected status code 201 but got 400
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

## Best Practices

1. **Environment-Specific Configuration**
   - Keep sensitive data in environment-specific files
   - Use environment variables for secrets
   - Never commit production credentials

2. **Test Data Management**
   - Store test data in JSON files
   - Use meaningful file names
   - Keep test data minimal and focused

3. **Test Organization**
   - Group related tests in test classes
   - Use descriptive test names
   - Follow the Arrange-Act-Assert pattern

4. **Error Handling**
   - Test both success and failure scenarios
   - Validate error responses
   - Include appropriate assertions

## Troubleshooting

### Common Issues

1. **Configuration Not Found**
   - Ensure configuration files are in the correct location
   - Check file names match environment names
   - Verify file properties are set to "Copy to Output Directory"

2. **API Connection Issues**
   - Verify API server is running
   - Check API base URL in configuration
   - Ensure network connectivity

3. **Test Data Issues**
   - Verify JSON file paths
   - Check JSON syntax
   - Ensure required fields are present

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 