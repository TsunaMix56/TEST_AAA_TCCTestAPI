using FluentAssertions;
using Moq;
using TCCTestAPI.Tests.Models;
using TCCTestAPI.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace TCCTestAPI.Tests.UnitTests;

/// <summary>
/// Unit tests for account creation service components using AAA pattern and mocking
/// </summary>
public class AccountCreationServiceUnitTests
{
    private readonly ITestOutputHelper _output;
    private readonly MockAccountCreationService _accountService;

    public AccountCreationServiceUnitTests(ITestOutputHelper output)
    {
        _output = output;
        _accountService = new MockAccountCreationService();
    }

    [Fact]
    public async Task CreateAccountAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Username = "mixtest",
            Password = "11223344"
        };
        var createdBy = "TCCTest";

        _output.WriteLine($"Testing account creation for username: {request.Username}");
        _output.WriteLine($"Created by: {createdBy}");

        // Act
        var result = await _accountService.CreateAccountAsync(request, createdBy);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be("Account created successfully");
        result.Username.Should().Be(request.Username);
        result.UserId.Should().NotBeNullOrEmpty();
        result.CreatedBy.Should().Be(createdBy);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _output.WriteLine($"Created account with ID: {result.UserId}");
        _output.WriteLine($"Created at: {result.CreatedAt}");
    }

    [Fact]
    public async Task CreateAccountAsync_WithExistingUsername_ShouldReturnFailureResponse()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Username = "TCCTest", // Existing username
            Password = "newpassword"
        };
        var createdBy = "admin";

        _output.WriteLine($"Testing duplicate username: {request.Username}");

        // Act
        var result = await _accountService.CreateAccountAsync(request, createdBy);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Username already exists");
        result.Username.Should().Be(request.Username);
        result.UserId.Should().BeNullOrEmpty();

        _output.WriteLine("Account creation failed as expected for existing username");
    }

    [Theory]
    [InlineData("validuser", true)]
    [InlineData("test123", true)]
    [InlineData("user.name", true)]
    [InlineData("user@email.com", true)]
    [InlineData("user_123", true)]
    [InlineData("", false)]
    [InlineData("a", false)]
    [InlineData("verylongusernamethatexceedslimitsabcdefghijklmnopqrstuvwxyz", false)]
    [InlineData("user@#$%", false)]
    [InlineData("user space", false)]
    public void ValidateUsername_WithVariousInputs_ShouldReturnExpectedResult(
        string username, bool expected)
    {
        // Arrange
        _output.WriteLine($"Testing username validation: '{username}'");

        // Act
        var result = _accountService.ValidateUsername(username);

        // Assert
        result.Should().Be(expected);
        _output.WriteLine($"Validation result: {result}");
    }

    [Theory]
    [InlineData("password123", true)]
    [InlineData("1234", true)]
    [InlineData("verylongpasswordwithmanycharacters", true)]
    [InlineData("", false)]
    [InlineData("123", false)]
    [InlineData("   ", false)]
    public void ValidatePassword_WithVariousInputs_ShouldReturnExpectedResult(
        string password, bool expected)
    {
        // Arrange
        _output.WriteLine($"Testing password validation with length: {password.Length}");

        // Act
        var result = _accountService.ValidatePassword(password);

        // Assert
        result.Should().Be(expected);
        _output.WriteLine($"Validation result: {result}");
    }

    [Theory]
    [InlineData("TCCTest", true)]
    [InlineData("admin", true)]
    [InlineData("existing_user", true)]
    [InlineData("newuser", false)]
    [InlineData("mixtest", false)]
    public async Task UsernameExistsAsync_WithVariousUsernames_ShouldReturnExpectedResult(
        string username, bool expected)
    {
        // Arrange
        _output.WriteLine($"Testing username existence check: '{username}'");

        // Act
        var result = await _accountService.UsernameExistsAsync(username);

        // Assert
        result.Should().Be(expected);
        _output.WriteLine($"Username exists: {result}");
    }

    [Fact]
    public async Task CreateAccountAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Username = "", // Invalid username
            Password = "validpassword"
        };
        var createdBy = "TCCTest";

        _output.WriteLine("Testing account creation with invalid username");

        // Act
        var result = await _accountService.CreateAccountAsync(request, createdBy);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("Account creation failed as expected for invalid username");
    }

    [Fact]
    public async Task CreateAccountAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Username = "validuser",
            Password = "123" // Invalid password (too short)
        };
        var createdBy = "TCCTest";

        _output.WriteLine("Testing account creation with invalid password");

        // Act
        var result = await _accountService.CreateAccountAsync(request, createdBy);

        // Assert
        result.Should().BeNull();
        _output.WriteLine("Account creation failed as expected for invalid password");
    }

    [Fact]
    public async Task CreateAccountAsync_MultipleValidAccounts_ShouldGenerateUniqueUserIds()
    {
        // Arrange
        var request1 = new CreateAccountRequest
        {
            Username = "user1",
            Password = "password1"
        };

        var request2 = new CreateAccountRequest
        {
            Username = "user2",
            Password = "password2"
        };

        var createdBy = "TCCTest";

        // Act
        var result1 = await _accountService.CreateAccountAsync(request1, createdBy);
        var result2 = await _accountService.CreateAccountAsync(request2, createdBy);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Success.Should().BeTrue();
        result2!.Success.Should().BeTrue();
        result1.UserId.Should().NotBe(result2.UserId, "User IDs should be unique");

        _output.WriteLine($"User 1 ID: {result1.UserId}");
        _output.WriteLine($"User 2 ID: {result2.UserId}");
    }
}

/// <summary>
/// Unit tests using Moq for more advanced mocking scenarios
/// </summary>
public class AccountCreationServiceMoqTests
{
    private readonly ITestOutputHelper _output;

    public AccountCreationServiceMoqTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task CreateAccountAsync_UsingMoq_ShouldCallValidationMethods()
    {
        // Arrange
        var mockService = new Mock<IAccountCreationService>();
        var request = new CreateAccountRequest
        {
            Username = "testuser",
            Password = "testpass"
        };
        var createdBy = "admin";

        var expectedResponse = new CreateAccountResponse
        {
            Success = true,
            Message = "Account created successfully",
            UserId = "123",
            Username = "testuser",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        };

        mockService.Setup(x => x.ValidateUsername(request.Username)).Returns(true);
        mockService.Setup(x => x.ValidatePassword(request.Password)).Returns(true);
        mockService.Setup(x => x.UsernameExistsAsync(request.Username)).ReturnsAsync(false);
        mockService.Setup(x => x.CreateAccountAsync(request, createdBy)).ReturnsAsync(expectedResponse);

        _output.WriteLine("Set up mock service with expected behavior");

        // Act
        var result = await mockService.Object.CreateAccountAsync(request, createdBy);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        // Verify that methods were called
        mockService.Verify(x => x.CreateAccountAsync(request, createdBy), Times.Once);

        _output.WriteLine($"Mock returned success: {result!.Success}");
    }

    [Fact]
    public void ValidateUsername_UsingMoq_ShouldReturnConfiguredResult()
    {
        // Arrange
        var mockService = new Mock<IAccountCreationService>();

        mockService.Setup(x => x.ValidateUsername("validuser")).Returns(true);
        mockService.Setup(x => x.ValidateUsername("")).Returns(false);
        mockService.Setup(x => x.ValidateUsername(It.IsAny<string>())).Returns(false);
        mockService.Setup(x => x.ValidateUsername("validuser")).Returns(true); // Override for specific case

        _output.WriteLine("Set up mock validation behavior");

        // Act
        var validResult = mockService.Object.ValidateUsername("validuser");
        var invalidResult = mockService.Object.ValidateUsername("");

        // Assert
        validResult.Should().BeTrue();
        invalidResult.Should().BeFalse();

        mockService.Verify(x => x.ValidateUsername(It.IsAny<string>()), Times.Exactly(2));

        _output.WriteLine($"Valid username result: {validResult}");
        _output.WriteLine($"Invalid username result: {invalidResult}");
    }

    [Fact]
    public async Task UsernameExistsAsync_UsingMoq_ShouldReturnMockedResult()
    {
        // Arrange
        var mockService = new Mock<IAccountCreationService>();
        var existingUsername = "existing";
        var newUsername = "newuser";

        mockService.Setup(x => x.UsernameExistsAsync(existingUsername)).ReturnsAsync(true);
        mockService.Setup(x => x.UsernameExistsAsync(newUsername)).ReturnsAsync(false);

        _output.WriteLine("Set up mock username existence checks");

        // Act
        var existsResult = await mockService.Object.UsernameExistsAsync(existingUsername);
        var notExistsResult = await mockService.Object.UsernameExistsAsync(newUsername);

        // Assert
        existsResult.Should().BeTrue();
        notExistsResult.Should().BeFalse();

        mockService.Verify(x => x.UsernameExistsAsync(existingUsername), Times.Once);
        mockService.Verify(x => x.UsernameExistsAsync(newUsername), Times.Once);

        _output.WriteLine($"Existing username check: {existsResult}");
        _output.WriteLine($"New username check: {notExistsResult}");
    }

    [Fact]
    public async Task CreateAccountAsync_UsingMoq_ShouldThrowExceptionForNullRequest()
    {
        // Arrange
        var mockService = new Mock<IAccountCreationService>();

        mockService.Setup(x => x.CreateAccountAsync(null!, It.IsAny<string>()))
                   .ThrowsAsync(new ArgumentNullException(nameof(CreateAccountRequest)));

        _output.WriteLine("Set up mock to throw exception for null request");

        // Act & Assert
        await mockService.Object.Invoking(async x => await x.CreateAccountAsync(null!, "admin"))
                        .Should().ThrowAsync<ArgumentNullException>();

        mockService.Verify(x => x.CreateAccountAsync(null!, It.IsAny<string>()), Times.Once);
        _output.WriteLine("Exception thrown as expected for null request");
    }
}