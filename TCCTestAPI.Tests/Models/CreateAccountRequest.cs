using System.ComponentModel.DataAnnotations;

namespace TCCTestAPI.Tests.Models;

/// <summary>
/// Request model for account creation API endpoint
/// </summary>
public class CreateAccountRequest
{
    /// <summary>
    /// The username for the new account
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for the new account
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}