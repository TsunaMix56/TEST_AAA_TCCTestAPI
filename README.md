# TCC Test API - Unit Testing Project

## Overview

This project contains comprehensive unit and integration tests for the TCC Authentication API using C# .NET 9 and the AAA (Arrange-Act-Assert) testing pattern.

## 🎯 Features

- **Integration Tests**: Direct testing of the live API endpoint at `http://localhost:5214/api/auth/token`
- **Unit Tests**: Isolated testing with mocking using Moq and custom mock services
- **AAA Pattern**: All tests follow the Arrange-Act-Assert pattern for clarity and maintainability
- **Comprehensive Coverage**: Tests for success scenarios, error cases, edge cases, and performance
- **WireMock Support**: Mock API server for isolated testing scenarios
- **Configurable Settings**: JSON-based configuration for test parameters

## 🛠️ Technologies Used

- **.NET 9**: Latest .NET framework
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable and expressive assertions
- **Moq**: Mocking framework for dependency injection
- **WireMock.NET**: HTTP API mocking
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core integration testing

## 📁 Project Structure

```
TCCTestAPI.Tests/
├── IntegrationTests/
│   └── AuthenticationApiIntegrationTests.cs    # Live API integration tests
├── UnitTests/
│   └── AuthenticationServiceUnitTests.cs       # Unit tests with mocking
├── Models/
│   ├── AuthenticationRequest.cs                # Request DTO
│   └── AuthenticationResponse.cs               # Response DTO
├── TestHelpers/
│   ├── MockAuthenticationService.cs            # Mock service implementation
│   ├── TestConfiguration.cs                    # Configuration helper
│   └── MockApiServer.cs                        # WireMock server setup
├── appsettings.test.json                       # Test configuration
├── TCCTestAPI.Tests.csproj                     # Project file
└── README.md                                   # This file
```

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code
- The target API running on `http://localhost:5214`

### Installation

1. **Clone or create the project**:
   ```bash
   git clone https://github.com/TsunaMix56/TEST_AAA_TCCTestAPI.git
   cd TEST_AAA_TCCTestAPI/TCCTestAPI.Tests
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

### Running Tests

#### Run All Tests
```bash
dotnet test
```

#### Run with Verbose Output
```bash
dotnet test --verbosity normal
```

#### Run Specific Test Categories
```bash
# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"
```

#### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📋 Test Cases

### Integration Tests (`AuthenticationApiIntegrationTests`)

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| `PostAuthToken_WithValidCredentials_ShouldReturnValidJwtToken` | Test successful authentication with valid credentials | HTTP 200, valid JWT token |
| `PostAuthToken_WithValidCredentials_ShouldReturnValidJwtStructure` | Validate JWT token structure (header.payload.signature) | Valid JWT format |
| `PostAuthToken_WithMissingCredentials_ShouldReturnBadRequest` | Test with empty username/password | HTTP 400/401 |
| `PostAuthToken_WithInvalidCredentials_ShouldReturnUnauthorized` | Test with wrong credentials | HTTP 401 |
| `PostAuthToken_WithInvalidJsonContent_ShouldReturnBadRequest` | Test with malformed JSON | HTTP 400 |
| `PostAuthToken_WithMissingContentType_ShouldReturnBadRequest` | Test without Content-Type header | HTTP 400/415 |
| `PostAuthToken_WithNullBody_ShouldReturnBadRequest` | Test with empty request body | HTTP 400 |
| `PostAuthToken_ResponseTime_ShouldBeLessThan5Seconds` | Performance test | Response < 5 seconds |
| `PostAuthToken_MultipleRequests_ShouldGenerateDifferentTokens` | Test token uniqueness | Different tokens per request |

### Unit Tests (`AuthenticationServiceUnitTests`)

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| `AuthenticateAsync_WithValidCredentials_ShouldReturnValidResponse` | Test service authentication logic | Valid response object |
| `AuthenticateAsync_WithInvalidCredentials_ShouldReturnNull` | Test failed authentication | Null return |
| `ValidateCredentials_WithVariousInputs_ShouldReturnExpectedResult` | Test credential validation | Boolean results |
| `GenerateJwtToken_WithValidUsername_ShouldReturnValidJwtStructure` | Test JWT generation | Valid JWT structure |
| `GenerateJwtToken_MultipleCalls_ShouldGenerateDifferentTokens` | Test token uniqueness | Unique tokens |

## 🔧 Configuration

### Test Settings (`appsettings.test.json`)

```json
{
  "TestSettings": {
    "ApiBaseUrl": "http://localhost:5214",
    "AuthEndpoint": "/api/auth/token",
    "DefaultTimeout": 30000,
    "RetryAttempts": 3,
    "ValidCredentials": {
      "Username": "TCCTest",
      "Password": "Test!456"
    }
  }
}
```

### Environment Variables

You can override configuration using environment variables:

```bash
export TestSettings__ApiBaseUrl="http://localhost:8080"
export TestSettings__ValidCredentials__Username="YourUsername"
export TestSettings__ValidCredentials__Password="YourPassword"
```

## 🧪 API Contract

### Request Format
```json
{
  "username": "TCCTest",
  "password": "Test!456"
}
```

### Success Response (HTTP 200)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2025-10-29T16:08:33.6729015Z",
  "username": "TCCTest",
  "message": "Welcome Admin: TCCTest"
}
```

### Error Response (HTTP 401)
```json
{
  "message": "Invalid credentials"
}
```

## 🎨 AAA Pattern Examples

### Arrange-Act-Assert Structure

```csharp
[Fact]
public async Task PostAuthToken_WithValidCredentials_ShouldReturnValidJwtToken()
{
    // Arrange - Set up test data and dependencies
    var request = new AuthenticationRequest
    {
        Username = "TCCTest",
        Password = "Test!456"
    };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act - Execute the operation being tested
    var response = await _httpClient.PostAsync("/api/auth/token", content);
    var responseContent = await response.Content.ReadAsStringAsync();

    // Assert - Verify the results
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent, _jsonOptions);
    authResponse.Should().NotBeNull();
    authResponse!.Token.Should().NotBeNullOrEmpty();
}
```

## 🏃‍♂️ Running the API

Before running integration tests, ensure your API is running:

```bash
# Start your API server
dotnet run --project YourApiProject
```

The API should be accessible at `http://localhost:5214/api/auth/token`

## 📊 Test Output

### Example Test Run Output
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

  Passed!  - Failed:     0, Passed:     12, Skipped:     0, Total:     12, Duration: 2 s

Test Run Successful.
Total tests: 12
     Passed: 12
 Total time: 2.3456 Seconds
```

### With Verbose Logging
```
[ 00:00:00.45]     TCCTestAPI.Tests.IntegrationTests.AuthenticationApiIntegrationTests.PostAuthToken_WithValidCredentials_ShouldReturnValidJwtToken [PASS]
[ 00:00:00.89]     TCCTestAPI.Tests.UnitTests.AuthenticationServiceUnitTests.AuthenticateAsync_WithValidCredentials_ShouldReturnValidResponse [PASS]
```

## 🐛 Troubleshooting

### Common Issues

1. **API Not Running**
   ```
   System.Net.Http.HttpRequestException: No connection could be made because the target machine actively refused it.
   ```
   **Solution**: Ensure your API is running on `http://localhost:5214`

2. **Port Conflict**
   ```
   Address already in use
   ```
   **Solution**: Update the `ApiBaseUrl` in `appsettings.test.json`

3. **SSL Certificate Issues**
   ```
   The SSL connection could not be established
   ```
   **Solution**: Use HTTP instead of HTTPS in test configuration

4. **Package Restore Issues**
   ```bash
   dotnet clean
   dotnet restore --force
   dotnet build
   ```

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/new-test`
3. **Follow AAA pattern**: Ensure all tests follow Arrange-Act-Assert
4. **Add documentation**: Update README for new test cases
5. **Run all tests**: Verify nothing is broken
6. **Submit a pull request**

### Code Style Guidelines

- Use descriptive test method names
- Follow the naming pattern: `MethodName_StateUnderTest_ExpectedBehavior`
- Add XML documentation for complex test helpers
- Use FluentAssertions for readable assertions
- Include test output logging for debugging

## 📄 License

This project is licensed under the MIT License.

## 📞 Support

For questions or issues:
- Create an issue in the repository
- Contact the development team
- Check the troubleshooting section above

---

**Happy Testing! 🧪✨**