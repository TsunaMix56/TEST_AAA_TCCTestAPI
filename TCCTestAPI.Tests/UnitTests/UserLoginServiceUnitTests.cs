using System.Linq;
using FluentAssertions;
using Moq;
using TCCTestAPI.Tests.Models;
using TCCTestAPI.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace TCCTestAPI.Tests.UnitTests;

/// <summary>
/// Unit tests for User Login service components using AAA pattern and mocking
/// </summary>
public class UserLoginServiceUnitTests
{
    private readonly ITestOutputHelper _output;
    private readonly MockUserLoginService _userLoginService;

    public UserLoginServiceUnitTests(ITestOutputHelper output)
    {
        _output = output;
        _userLoginService = new MockUserLoginService();
    }

    [Fact]
    public async Task UserLoginAsync_WithValidCredentials_ShouldReturnWelcomeMessage()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Username = "mixtest",
            Password = "11223344"
        };

        _output.WriteLine($"Testing user login with username: {request.Username}");

        // Act
        var result = await _userLoginService.UserLoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be($"Welcome User: {request.Username}");

        _output.WriteLine($"Login result: {result.Message}");
    }

    [Fact]
    public async Task UserLoginAsync_WithInvalidCredentials_ShouldReturnNull()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };

        _output.WriteLine($"Testing user login with invalid credentials: {request.Username}");

        // Act
        var result = await _userLoginService.UserLoginAsync(request);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("User login failed as expected");
    }

    [Theory]
    [InlineData("mixtest", "11223344", true)]
    [InlineData("testuser1", "password123", true)]
    [InlineData("adminuser", "admin2024", true)]
    [InlineData("mixtest", "wrongpassword", false)]
    [InlineData("wronguser", "11223344", false)]
    [InlineData("", "", false)]
    [InlineData(null, "password", false)]
    [InlineData("username", null, false)]
    public void ValidateUserCredentials_WithVariousInputs_ShouldReturnExpectedResult(
        string username, string password, bool expected)
    {
        // Arrange
        _output.WriteLine($"Testing user credentials: {username} / {password}");

        // Act
        var result = _userLoginService.ValidateUserCredentials(username, password);

        // Assert
        result.Should().Be(expected);
        _output.WriteLine($"Validation result: {result}");
    }

    [Fact]
    public void ValidateUserCredentials_WithNullUsername_ShouldReturnFalse()
    {
        // Arrange
        string username = null!;
        string password = "password";

        _output.WriteLine("Testing user credentials with null username");

        // Act
        var result = _userLoginService.ValidateUserCredentials(username, password);

        // Assert
        result.Should().BeFalse();
        _output.WriteLine($"Validation result for null username: {result}");
    }

    [Fact]
    public void ValidateUserCredentials_WithNullPassword_ShouldReturnFalse()
    {
        // Arrange
        string username = "testuser";
        string password = null!;

        _output.WriteLine("Testing user credentials with null password");

        // Act
        var result = _userLoginService.ValidateUserCredentials(username, password);

        // Assert
        result.Should().BeFalse();
        _output.WriteLine($"Validation result for null password: {result}");
    }

    [Fact]
    public void ValidateUserCredentials_WithEmptyStrings_ShouldReturnFalse()
    {
        // Arrange
        string username = "";
        string password = "";

        _output.WriteLine("Testing user credentials with empty strings");

        // Act
        var result = _userLoginService.ValidateUserCredentials(username, password);

        // Assert
        result.Should().BeFalse();
        _output.WriteLine($"Validation result for empty credentials: {result}");
    }

    [Fact]
    public void ValidateUserCredentials_WithWhitespaceOnly_ShouldReturnFalse()
    {
        // Arrange
        string username = "   ";
        string password = "   ";

        _output.WriteLine("Testing user credentials with whitespace only");

        // Act
        var result = _userLoginService.ValidateUserCredentials(username, password);

        // Assert
        result.Should().BeFalse();
        _output.WriteLine($"Validation result for whitespace credentials: {result}");
    }

    [Fact]
    public async Task UserLoginAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserLoginRequest request = null!;

        _output.WriteLine("Testing user login with null request");

        // Act & Assert
        await _userLoginService.Invoking(async x => await x.UserLoginAsync(request))
                              .Should().ThrowAsync<ArgumentNullException>();

        _output.WriteLine("ArgumentNullException thrown as expected");
    }

    [Fact]
    public async Task UserLoginAsync_WithEmptyUsername_ShouldReturnNull()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Username = "",
            Password = "11223344"
        };

        _output.WriteLine("Testing user login with empty username");

        // Act
        var result = await _userLoginService.UserLoginAsync(request);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("User login failed as expected for empty username");
    }

    [Fact]
    public async Task UserLoginAsync_WithEmptyPassword_ShouldReturnNull()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Username = "mixtest",
            Password = ""
        };

        _output.WriteLine("Testing user login with empty password");

        // Act
        var result = await _userLoginService.UserLoginAsync(request);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("User login failed as expected for empty password");
    }
}

/// <summary>
/// Unit tests using Moq for more advanced User Login mocking scenarios
/// </summary>
public class UserLoginServiceMoqTests
{
    private readonly ITestOutputHelper _output;

    public UserLoginServiceMoqTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task UserLoginAsync_UsingMoq_ShouldReturnExpectedResponse()
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();
        var request = new UserLoginRequest
        {
            Username = "mixtest",
            Password = "11223344"
        };

        var expectedResponse = new UserLoginResponse
        {
            Message = "Welcome User: mixtest"
        };

        mockService.Setup(x => x.ValidateUserCredentials(request.Username, request.Password))
                   .Returns(true);

        mockService.Setup(x => x.UserLoginAsync(request))
                   .ReturnsAsync(expectedResponse);

        _output.WriteLine("Set up mock service with expected behavior");

        // Act
        var result = await mockService.Object.UserLoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        // Verify that methods were called
        mockService.Verify(x => x.UserLoginAsync(request), Times.Once);

        _output.WriteLine($"Mock returned: {result!.Message}");
    }

    [Fact]
    public void ValidateUserCredentials_UsingMoq_ShouldReturnConfiguredResult()
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();
        var username = "mixtest";
        var password = "11223344";

        mockService.Setup(x => x.ValidateUserCredentials(username, password))
                   .Returns(true);

        mockService.Setup(x => x.ValidateUserCredentials(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(false);

        mockService.Setup(x => x.ValidateUserCredentials(username, password))
                   .Returns(true); // Override for specific credentials

        _output.WriteLine("Set up mock validation behavior");

        // Act
        var validResult = mockService.Object.ValidateUserCredentials(username, password);
        var invalidResult = mockService.Object.ValidateUserCredentials("wrong", "wrong");

        // Assert
        validResult.Should().BeTrue();
        invalidResult.Should().BeFalse();

        // Verify call counts
        mockService.Verify(x => x.ValidateUserCredentials(It.IsAny<string>(), It.IsAny<string>()),
                          Times.Exactly(2));

        _output.WriteLine($"Valid credentials result: {validResult}");
        _output.WriteLine($"Invalid credentials result: {invalidResult}");
    }

    [Fact]
    public async Task UserLoginAsync_UsingMoq_ShouldThrowExceptionForNullRequest()
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();

        mockService.Setup(x => x.UserLoginAsync(null!))
                   .ThrowsAsync(new ArgumentNullException(nameof(UserLoginRequest)));

        _output.WriteLine("Set up mock to throw exception for null request");

        // Act & Assert
        await mockService.Object.Invoking(async x => await x.UserLoginAsync(null!))
                        .Should().ThrowAsync<ArgumentNullException>();

        mockService.Verify(x => x.UserLoginAsync(null!), Times.Once);
        _output.WriteLine("Exception thrown as expected");
    }

    [Theory]
    [InlineData("mixtest", "11223344", "Welcome User: mixtest")]
    [InlineData("testuser1", "password123", "Welcome User: testuser1")]
    [InlineData("adminuser", "admin2024", "Welcome User: adminuser")]
    public async Task UserLoginAsync_UsingMoq_WithVariousUsers_ShouldReturnCorrectMessages(
        string username, string password, string expectedMessage)
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();
        var request = new UserLoginRequest
        {
            Username = username,
            Password = password
        };

        var expectedResponse = new UserLoginResponse
        {
            Message = expectedMessage
        };

        mockService.Setup(x => x.ValidateUserCredentials(username, password))
                   .Returns(true);

        mockService.Setup(x => x.UserLoginAsync(It.Is<UserLoginRequest>(r =>
                            r.Username == username && r.Password == password)))
                   .ReturnsAsync(expectedResponse);

        _output.WriteLine($"Testing mock for user: {username}");

        // Act
        var result = await mockService.Object.UserLoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be(expectedMessage);

        mockService.Verify(x => x.UserLoginAsync(It.Is<UserLoginRequest>(r =>
                            r.Username == username && r.Password == password)), Times.Once);

        _output.WriteLine($"Mock test passed for user: {username} with message: {result.Message}");
    }

    [Fact]
    public async Task UserLoginAsync_UsingMoq_ShouldVerifyMethodCallSequence()
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();
        var request = new UserLoginRequest
        {
            Username = "mixtest",
            Password = "11223344"
        };

        var callSequence = new MockSequence();

        mockService.InSequence(callSequence)
                   .Setup(x => x.ValidateUserCredentials(request.Username, request.Password))
                   .Returns(true);

        mockService.InSequence(callSequence)
                   .Setup(x => x.UserLoginAsync(request))
                   .ReturnsAsync(new UserLoginResponse { Message = "Welcome User: mixtest" });

        _output.WriteLine("Set up mock with call sequence verification");

        // Act
        var isValid = mockService.Object.ValidateUserCredentials(request.Username, request.Password);
        var result = await mockService.Object.UserLoginAsync(request);

        // Assert
        isValid.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Message.Should().Be("Welcome User: mixtest");

        // Verify the sequence
        mockService.Verify(x => x.ValidateUserCredentials(request.Username, request.Password), Times.Once);
        mockService.Verify(x => x.UserLoginAsync(request), Times.Once);

        _output.WriteLine("Method call sequence verified successfully");
    }

    [Fact]
    public void ValidateUserCredentials_UsingMoq_ShouldSupportComplexMatching()
    {
        // Arrange
        var mockService = new Mock<IUserLoginService>();

        // Setup using callback pattern for complex matching - this approach works better than multiple setups
        mockService.Setup(x => x.ValidateUserCredentials(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns<string, string>((username, password) =>
                   {
                       // Define specific valid combinations
                       var validCombinations = new[]
                       {
                           ("mixtest", "12345678"),
                           ("test", "12345678"),
                           ("mixtest", "123")
                       };

                       return validCombinations.Any(combo =>
                           combo.Item1 == username && combo.Item2 == password);
                   });

        _output.WriteLine("Set up complex matching rules using callback pattern");

        // Act & Assert
        mockService.Object.ValidateUserCredentials("mixtest", "12345678").Should().BeTrue();
        mockService.Object.ValidateUserCredentials("test", "12345678").Should().BeTrue();
        mockService.Object.ValidateUserCredentials("mixtest", "123").Should().BeTrue();
        mockService.Object.ValidateUserCredentials("test", "123").Should().BeFalse();

        _output.WriteLine("Complex matching verification completed");
    }
}