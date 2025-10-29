namespace TCCTestAPI.Tests.Models;

/// <summary>
/// Response model for authentication API endpoint
/// </summary>
public class AuthenticationResponse
{
    /// <summary>
    /// The JWT token returned by the authentication API
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The expiration date and time of the token
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// The username that was authenticated
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Welcome message for the authenticated user
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response model for user login API endpoint
/// </summary>
public class UserLoginResponse
{
    /// <summary>
    /// Welcome message for the user
    /// </summary>
    public string Message { get; set; } = string.Empty;
}