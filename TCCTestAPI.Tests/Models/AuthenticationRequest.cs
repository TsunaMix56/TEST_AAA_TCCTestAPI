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