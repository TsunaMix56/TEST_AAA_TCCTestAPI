using TCCTestAPI.Tests.Models;

namespace TCCTestAPI.Tests.TestHelpers;

/// <summary>
/// Interface for User Login Service operations
/// </summary>
public interface IUserLoginService
{
    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    /// <param name="request">User login request containing credentials</param>
    /// <returns>User login response with welcome message or null if authentication fails</returns>
    Task<UserLoginResponse?> UserLoginAsync(UserLoginRequest request);

    /// <summary>
    /// Validate user credentials
    /// </summary>
    /// <param name="username">The username to validate</param>
    /// <param name="password">The password to validate</param>
    /// <returns>True if credentials are valid, false otherwise</returns>
    bool ValidateUserCredentials(string username, string password);
}

/// <summary>
/// Mock implementation of User Login Service for testing purposes
/// </summary>
public class MockUserLoginService : IUserLoginService
{
    private readonly Dictionary<string, string> _validUserCredentials;

    public MockUserLoginService()
    {
        // Initialize with test user credentials
        _validUserCredentials = new Dictionary<string, string>
        {
            { "mixtest", "11223344" },
            { "testuser1", "password123" },
            { "adminuser", "admin2024" },
            { "user1", "pass1" },
            { "user2", "pass2" },
            { "user3", "pass3" },
            { "demouser", "demo123" },
            { "qauser", "testing2024" }
        };
    }

    /// <summary>
    /// Mock user login implementation
    /// </summary>
    public async Task<UserLoginResponse?> UserLoginAsync(UserLoginRequest request)
    {
        // Validate input
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Simulate async operation
        await Task.Delay(1);

        // Validate credentials
        if (!ValidateUserCredentials(request.Username, request.Password))
            return null;

        // Return successful response
        return new UserLoginResponse
        {
            Message = $"Welcome User: {request.Username}"
        };
    }

    /// <summary>
    /// Validate user credentials against mock user database
    /// </summary>
    public bool ValidateUserCredentials(string username, string password)
    {
        // Check for null or empty values
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        // Check against valid credentials
        return _validUserCredentials.TryGetValue(username, out var expectedPassword) &&
               expectedPassword == password;
    }

    /// <summary>
    /// Add a new user to the mock database (for testing purposes)
    /// </summary>
    public void AddTestUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Username and password cannot be null or empty");

        _validUserCredentials[username] = password;
    }

    /// <summary>
    /// Remove a user from the mock database (for testing purposes)
    /// </summary>
    public void RemoveTestUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return;

        _validUserCredentials.Remove(username);
    }

    /// <summary>
    /// Get all registered test users (for testing purposes)
    /// </summary>
    public IEnumerable<string> GetTestUsers()
    {
        return _validUserCredentials.Keys;
    }

    /// <summary>
    /// Clear all test users (for testing purposes)
    /// </summary>
    public void ClearTestUsers()
    {
        _validUserCredentials.Clear();
    }

    /// <summary>
    /// Check if a user exists in the mock database
    /// </summary>
    public bool UserExists(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return _validUserCredentials.ContainsKey(username);
    }
}