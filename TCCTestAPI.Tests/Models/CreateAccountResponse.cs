namespace TCCTestAPI.Tests.Models;

/// <summary>
/// Response model for account creation API endpoint
/// </summary>
public class CreateAccountResponse
{
    /// <summary>
    /// Indicates if the account creation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result of the operation
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the created user
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The username of the created account
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The user who created this account
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}