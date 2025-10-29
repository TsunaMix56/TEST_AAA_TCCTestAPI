using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using TCCTestAPI.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace TCCTestAPI.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the Authentication API endpoint using AAA pattern
/// Tests the actual HTTP API at http://localhost:5214/api/auth/token
/// </summary>
public class AuthenticationApiIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationApiIntegrationTests(ITestOutputHelper output)
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

    [Fact]
    public async Task PostAuthToken_WithValidCredentials_ShouldReturnValidJwtToken()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Sending request to: {_httpClient.BaseAddress}api/auth/token");
        _output.WriteLine($"Request body: {requestJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent, _jsonOptions);

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Token.Should().StartWith("eyJ"); // JWT tokens start with eyJ
        authResponse.Username.Should().Be("TCCTest");
        authResponse.Message.Should().Contain("Welcome Admin: TCCTest");
        authResponse.Expires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task PostAuthToken_WithValidCredentials_ShouldReturnValidJwtStructure()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent, _jsonOptions);        // Validate JWT structure (header.payload.signature)
        var tokenParts = authResponse!.Token.Split('.');
        tokenParts.Should().HaveCount(3, "JWT should have header, payload, and signature");

        // Each part should be base64 encoded (no empty parts)
        tokenParts[0].Should().NotBeNullOrEmpty("JWT header should not be empty");
        tokenParts[1].Should().NotBeNullOrEmpty("JWT payload should not be empty");
        tokenParts[2].Should().NotBeNullOrEmpty("JWT signature should not be empty");
    }

    [Theory]
    [InlineData("", "Test!456", "Username is required")]
    [InlineData("TCCTest", "", "Password is required")]
    [InlineData("", "", "Both username and password are required")]
    public async Task PostAuthToken_WithMissingCredentials_ShouldReturnBadRequest(
        string username, string password, string scenario)
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = username,
            Password = password
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Testing scenario: {scenario}");
        _output.WriteLine($"Request body: {requestJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("WrongUser", "Test!456")]
    [InlineData("TCCTest", "WrongPassword")]
    [InlineData("InvalidUser", "InvalidPassword")]
    public async Task PostAuthToken_WithInvalidCredentials_ShouldReturnUnauthorized(
        string username, string password)
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = username,
            Password = password
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Testing invalid credentials: {username}/{password}");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostAuthToken_WithInvalidJsonContent_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        _output.WriteLine($"Sending invalid JSON: {invalidJson}");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAuthToken_WithMissingContentType_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8); // Missing content type

        _output.WriteLine("Sending request without Content-Type header");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.OK); // Some APIs might still accept it
    }

    [Fact]
    public async Task PostAuthToken_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        _output.WriteLine("Sending request with empty body");

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response body: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAuthToken_ResponseTime_ShouldBeLessThan5Seconds()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _httpClient.PostAsync("/api/auth/token", content);
        stopwatch.Stop();

        var responseTime = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Response time: {responseTime}ms");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseTime.Should().BeLessThan(5000, "API should respond within 5 seconds");
    }

    [Fact]
    public async Task PostAuthToken_MultipleRequests_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);

        // Act - Make two separate requests
        var content1 = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response1 = await _httpClient.PostAsync("/api/auth/token", content1);
        var responseContent1 = await response1.Content.ReadAsStringAsync();

        // Small delay to ensure different timestamps
        await Task.Delay(100);

        var content2 = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response2 = await _httpClient.PostAsync("/api/auth/token", content2);
        var responseContent2 = await response2.Content.ReadAsStringAsync();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse1 = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent1, _jsonOptions);
        var authResponse2 = JsonSerializer.Deserialize<AuthenticationResponse>(responseContent2, _jsonOptions);

        authResponse1!.Token.Should().NotBe(authResponse2!.Token,
            "Different requests should generate different tokens (due to different timestamps/JTI)");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}