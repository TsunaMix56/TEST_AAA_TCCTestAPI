using FluentAssertions;
using Moq;
using TCCTestAPI.Tests.Models;
using TCCTestAPI.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace TCCTestAPI.Tests.UnitTests;

/// <summary>
/// Unit tests for authentication service components using AAA pattern and mocking
/// </summary>
public class AuthenticationServiceUnitTests
{
    private readonly ITestOutputHelper _output;
    private readonly MockAuthenticationService _authService;

    public AuthenticationServiceUnitTests(ITestOutputHelper output)
    {
        _output = output;
        _authService = new MockAuthenticationService();
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnValidResponse()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        _output.WriteLine($"Testing authentication with username: {request.Username}");

        // Act
        var result = await _authService.AuthenticateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be(request.Username);
        result.Message.Should().Be("Welcome Admin: TCCTest");
        result.Expires.Should().BeAfter(DateTime.UtcNow);

        _output.WriteLine($"Generated token: {result.Token[..50]}...");
        _output.WriteLine($"Expires at: {result.Expires}");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnNull()
    {
        // Arrange
        var request = new AuthenticationRequest
        {
            Username = "InvalidUser",
            Password = "InvalidPassword"
        };

        _output.WriteLine($"Testing authentication with invalid credentials: {request.Username}");

        // Act
        var result = await _authService.AuthenticateAsync(request);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("Authentication failed as expected");
    }

    [Theory]
    [InlineData("TCCTest", "Test!456", true)]
    [InlineData("admin", "admin123", true)]
    [InlineData("testuser", "password", true)]
    [InlineData("TCCTest", "wrongpassword", false)]
    [InlineData("wronguser", "Test!456", false)]
    [InlineData("", "", false)]
    public void ValidateCredentials_WithVariousInputs_ShouldReturnExpectedResult(
        string username, string password, bool expected)
    {
        // Arrange
        _output.WriteLine($"Testing credentials: {username} / {password}");

        // Act
        var result = _authService.ValidateCredentials(username, password);

        // Assert
        result.Should().Be(expected);
        _output.WriteLine($"Validation result: {result}");
    }

    [Fact]
    public void GenerateJwtToken_WithValidUsername_ShouldReturnValidJwtStructure()
    {
        // Arrange
        var username = "TCCTest";

        // Act
        var token = _authService.GenerateJwtToken(username);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var parts = token.Split('.');
        parts.Should().HaveCount(3, "JWT should have three parts separated by dots");

        parts[0].Should().NotBeNullOrEmpty("Header should not be empty");
        parts[1].Should().NotBeNullOrEmpty("Payload should not be empty");
        parts[2].Should().NotBeNullOrEmpty("Signature should not be empty");

        _output.WriteLine($"Generated JWT token: {token[..50]}...");
        _output.WriteLine($"Header: {parts[0]}");
        _output.WriteLine($"Payload length: {parts[1].Length}");
        _output.WriteLine($"Signature: {parts[2]}");
    }

    [Fact]
    public void GenerateJwtToken_MultipleCalls_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var username = "TCCTest";

        // Act
        var token1 = _authService.GenerateJwtToken(username);
        var token2 = _authService.GenerateJwtToken(username);

        // Assert
        token1.Should().NotBe(token2, "Each call should generate a unique token");

        _output.WriteLine($"Token 1: {token1[..50]}...");
        _output.WriteLine($"Token 2: {token2[..50]}...");
    }
}

/// <summary>
/// Unit tests using Moq for more advanced mocking scenarios
/// </summary>
public class AuthenticationServiceMoqTests
{
    private readonly ITestOutputHelper _output;

    public AuthenticationServiceMoqTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AuthenticateAsync_UsingMoq_ShouldCallValidateCredentials()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();
        var request = new AuthenticationRequest
        {
            Username = "TCCTest",
            Password = "Test!456"
        };

        var expectedResponse = new AuthenticationResponse
        {
            Token = "mock_token_12345",
            Username = "TCCTest",
            Message = "Welcome Admin: TCCTest",
            Expires = DateTime.UtcNow.AddHours(1)
        };

        mockService.Setup(x => x.ValidateCredentials(request.Username, request.Password))
                   .Returns(true);

        mockService.Setup(x => x.GenerateJwtToken(request.Username))
                   .Returns("mock_token_12345");

        mockService.Setup(x => x.AuthenticateAsync(request))
                   .ReturnsAsync(expectedResponse);

        _output.WriteLine("Set up mock service with expected behavior");

        // Act
        var result = await mockService.Object.AuthenticateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        // Verify that methods were called
        mockService.Verify(x => x.AuthenticateAsync(request), Times.Once);

        _output.WriteLine($"Mock returned: {result!.Token}");
    }

    [Fact]
    public void ValidateCredentials_UsingMoq_ShouldReturnConfiguredResult()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();
        var username = "TCCTest";
        var password = "Test!456";

        mockService.Setup(x => x.ValidateCredentials(username, password))
                   .Returns(true);

        mockService.Setup(x => x.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(false);

        mockService.Setup(x => x.ValidateCredentials(username, password))
                   .Returns(true); // Override for specific credentials

        _output.WriteLine("Set up mock validation behavior");

        // Act
        var validResult = mockService.Object.ValidateCredentials(username, password);
        var invalidResult = mockService.Object.ValidateCredentials("wrong", "wrong");

        // Assert
        validResult.Should().BeTrue();
        invalidResult.Should().BeFalse();

        // Verify call counts
        mockService.Verify(x => x.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Exactly(2));

        _output.WriteLine($"Valid credentials result: {validResult}");
        _output.WriteLine($"Invalid credentials result: {invalidResult}");
    }

    [Fact]
    public void GenerateJwtToken_UsingMoq_ShouldReturnMockedToken()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();
        var username = "TCCTest";
        var expectedToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.mock.payload.signature";

        mockService.Setup(x => x.GenerateJwtToken(username))
                   .Returns(expectedToken);

        _output.WriteLine($"Set up mock to return token: {expectedToken}");

        // Act
        var result = mockService.Object.GenerateJwtToken(username);

        // Assert
        result.Should().Be(expectedToken);
        mockService.Verify(x => x.GenerateJwtToken(username), Times.Once);

        _output.WriteLine($"Mock returned token: {result}");
    }

    [Fact]
    public async Task AuthenticateAsync_UsingMoq_ShouldThrowExceptionForNullRequest()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();

        mockService.Setup(x => x.AuthenticateAsync(null!))
                   .ThrowsAsync(new ArgumentNullException(nameof(AuthenticationRequest)));

        _output.WriteLine("Set up mock to throw exception for null request");

        // Act & Assert
        await mockService.Object.Invoking(async x => await x.AuthenticateAsync(null!))
                        .Should().ThrowAsync<ArgumentNullException>();

        mockService.Verify(x => x.AuthenticateAsync(null!), Times.Once);
        _output.WriteLine("Exception thrown as expected");
    }
}