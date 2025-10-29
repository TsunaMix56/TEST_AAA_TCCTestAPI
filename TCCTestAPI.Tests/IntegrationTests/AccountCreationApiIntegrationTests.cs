using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using TCCTestAPI.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace TCCTestAPI.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the Account Creation API endpoint using AAA pattern
/// Tests the actual HTTP API at http://localhost:5214/api/account/create
/// </summary>
public class AccountCreationApiIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public AccountCreationApiIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5214")
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Helper method to get authentication token for authorized requests
    /// </summary>
    private async Task<string> GetAuthenticationTokenAsync()
    {
        var authRequest = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(authRequest, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/auth/token", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent, _jsonOptions);

        return authResponse?.Token ?? throw new InvalidOperationException("Failed to get authentication token");
    }

    [Fact(Skip = "API Server required at localhost:5214 - Enable when server is running")]
    public async Task PostAccountCreate_WithValidDataAndAuth_ShouldCreateAccountSuccessfully()
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = "mixtest",
            Password = "11223344"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Sending request to: {_httpClient.BaseAddress}api/account/create");
        _output.WriteLine($"Request body: {requestJson}");
        _output.WriteLine($"Using Bearer token: {token[..20]}...");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var accountResponse = JsonSerializer.Deserialize<CreateAccountResponse>(responseContent, _jsonOptions);

        accountResponse.Should().NotBeNull();
        accountResponse!.Success.Should().BeTrue();
        accountResponse.Message.Should().Be("Account created successfully");
        accountResponse.Username.Should().Be("mixtest");
        accountResponse.UserId.Should().NotBeNullOrEmpty();
        accountResponse.CreatedBy.Should().Be("TCCTest");
        accountResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task PostAccountCreate_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Username = "testuser",
            Password = "testpass"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine("Sending request without authentication token");
        _output.WriteLine($"Request body: {requestJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostAccountCreate_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.token";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        var request = new CreateAccountRequest
        {
            Username = "testuser",
            Password = "testpass"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine("Sending request with invalid authentication token");
        _output.WriteLine($"Invalid token: {invalidToken}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "password123", "Username is required")]
    [InlineData("username", "", "Password is required")]
    [InlineData("", "", "Both username and password are required")]
    public async Task PostAccountCreate_WithMissingRequiredFields_ShouldReturnBadRequest(
        string username, string password, string scenario)
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = username,
            Password = password
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Testing scenario: {scenario}");
        _output.WriteLine($"Request body: {requestJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAccountCreate_WithDuplicateUsername_ShouldReturnConflict()
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = "TCCTest", // Using existing username
            Password = "newpassword"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine("Testing duplicate username scenario");
        _output.WriteLine($"Request body: {requestJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAccountCreate_WithInvalidJsonContent_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Sending invalid JSON: {invalidJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAccountCreate_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange - Using a token that should be expired (very old timestamp)
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IlRDQ1Rlc3QiLCJuYW1laWQiOiJUQ0NUZXN0Iiwic3ViIjoiVENDVGVzdCIsImp0aSI6IjM4OGZiODRkLWQzY2QtNGYxOS04MzBjLWU5MzEyY2NlMjliNSIsImlhdCI6MTUwMDAwMDAwMCwibmJmIjoxNTAwMDAwMDAwLCJleHAiOjE1MDAwMDM2MDAsImlzcyI6IlRDQ0p3dEFwaSIsImF1ZCI6IlRDQ0p3dEFwaVVzZXJzIn0.invalid_signature";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var request = new CreateAccountRequest
        {
            Username = "testuser",
            Password = "testpass"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine("Testing with expired token");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "API Server required at localhost:5214 - Enable when server is running")]
    public async Task PostAccountCreate_ResponseTime_ShouldBeLessThan5Seconds()
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = $"speedtest{DateTime.Now.Ticks}",
            Password = "testpass123"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        stopwatch.Stop();

        var responseTime = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Response time: {responseTime}ms");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict); // OK if created, Conflict if username exists
        responseTime.Should().BeLessThan(5000, "API should respond within 5 seconds");
    }

    [Theory(Skip = "API Server required at localhost:5214 - Enable when server is running")]
    [InlineData("u", "12345678", "Username too short")]
    [InlineData("verylongusernamethatexceedslimits12345678901234567890", "password", "Username too long")]
    [InlineData("validuser", "123", "Password too short")]
    public async Task PostAccountCreate_WithInvalidFieldLengths_ShouldHandleAppropriately(
        string username, string password, string scenario)
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = username,
            Password = password
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Testing scenario: {scenario}");
        _output.WriteLine($"Username length: {username.Length}, Password length: {password.Length}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK); // Some APIs might accept these, others might reject
    }

    [Fact]
    public async Task PostAccountCreate_WithSpecialCharactersInUsername_ShouldHandleCorrectly()
    {
        // Arrange
        var token = await GetAuthenticationTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest
        {
            Username = "test@user.com",
            Password = "password123"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine("Testing username with special characters");
        _output.WriteLine($"Username: {request.Username}");

        // Act
        var response = await _httpClient.PostAsync("/api/account/create", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict); // Depending on API validation rules
    }

    public void Dispose()
    {
        if (_httpClient != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null; // Clear auth header
            _httpClient.Dispose();
        }
    }
}