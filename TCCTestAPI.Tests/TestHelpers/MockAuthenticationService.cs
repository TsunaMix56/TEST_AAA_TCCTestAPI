using TCCTestAPI.Tests.Models;

namespace TCCTestAPI.Tests.TestHelpers;

/// <summary>
/// Interface for authentication service to enable mocking
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    bool ValidateCredentials(string username, string password);
    string GenerateJwtToken(string username);
}

/// <summary>
/// Mock implementation of authentication service for testing
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    private readonly Dictionary<string, string> _validCredentials;

    public MockAuthenticationService()
    {
        _validCredentials = new Dictionary<string, string>
        {
            { "TCCTest", "Test!456" },
            { "admin", "admin123" },
            { "testuser", "password" }
        };
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        await Task.Delay(10); // Simulate async operation

        if (!ValidateCredentials(request.Username, request.Password))
        {
            return null;
        }

        var token = GenerateJwtToken(request.Username);
        return new AuthenticationResponse
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1),
            Username = request.Username,
            Message = $"Welcome Admin: {request.Username}"
        };
    }

    public bool ValidateCredentials(string username, string password)
    {
        return _validCredentials.ContainsKey(username) &&
               _validCredentials[username] == password;
    }

    public string GenerateJwtToken(string username)
    {
        // Mock JWT token structure (header.payload.signature)
        var header = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{{\"unique_name\":\"{username}\",\"nameid\":\"{username}\",\"sub\":\"{username}\",\"jti\":\"{Guid.NewGuid()}\",\"iat\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"nbf\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()},\"iss\":\"TCCJwtApi\",\"aud\":\"TCCJwtApiUsers\"}}"));
        var signature = "mock_signature_" + Guid.NewGuid().ToString("N")[..16];

        return $"{header}.{payload}.{signature}";
    }
}