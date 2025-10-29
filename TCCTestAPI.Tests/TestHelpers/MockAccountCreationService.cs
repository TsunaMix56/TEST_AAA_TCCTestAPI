using TCCTestAPI.Tests.Models;

namespace TCCTestAPI.Tests.TestHelpers;

/// <summary>
/// Interface for account creation service to enable mocking
/// </summary>
public interface IAccountCreationService
{
    Task<CreateAccountResponse?> CreateAccountAsync(CreateAccountRequest request, string createdBy);
    bool ValidateUsername(string username);
    bool ValidatePassword(string password);
    Task<bool> UsernameExistsAsync(string username);
}

/// <summary>
/// Mock implementation of account creation service for testing
/// </summary>
public class MockAccountCreationService : IAccountCreationService
{
    private readonly HashSet<string> _existingUsernames;
    private int _nextUserId = 1;

    public MockAccountCreationService()
    {
        _existingUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TCCTest",
            "admin",
            "existing_user"
        };
    }

    public async Task<CreateAccountResponse?> CreateAccountAsync(CreateAccountRequest request, string createdBy)
    {
        await Task.Delay(50); // Simulate async operation

        if (!ValidateUsername(request.Username) || !ValidatePassword(request.Password))
        {
            return null;
        }

        if (await UsernameExistsAsync(request.Username))
        {
            return new CreateAccountResponse
            {
                Success = false,
                Message = "Username already exists",
                Username = request.Username,
                CreatedBy = createdBy
            };
        }

        // Add to existing usernames for future checks
        _existingUsernames.Add(request.Username);

        return new CreateAccountResponse
        {
            Success = true,
            Message = "Account created successfully",
            UserId = _nextUserId++.ToString(),
            Username = request.Username,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public bool ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        if (username.Length < 2 || username.Length > 50)
            return false;

        // Allow alphanumeric, underscore, dot, and @ symbol
        return System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_.@]+$");
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Minimum 4 characters for testing purposes
        return password.Length >= 4;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        await Task.Delay(10); // Simulate database check
        return _existingUsernames.Contains(username);
    }
}