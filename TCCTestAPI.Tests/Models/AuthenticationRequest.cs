using System.ComponentModel.DataAnnotations;

namespace TCCTestAPI.Tests.Models;

/// <summary>
/// Request model for authentication API endpoint
/// </summary>
public class AuthenticationRequest
{
    /// <summary>
    /// The username for authentication
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for authentication
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for user login API endpoint
/// </summary>
public class UserLoginRequest
{
    /// <summary>
    /// The username for user login
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for user login
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}