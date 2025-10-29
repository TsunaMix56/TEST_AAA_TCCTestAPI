using Microsoft.Extensions.Configuration;

namespace TCCTestAPI.Tests.TestHelpers;

/// <summary>
/// Test configuration helper to load and manage test settings
/// </summary>
public class TestConfiguration
{
    private readonly IConfiguration _configuration;

    public TestConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    public string ApiBaseUrl => _configuration["TestSettings:ApiBaseUrl"] ?? "http://localhost:5214";
    public string AuthEndpoint => _configuration["TestSettings:AuthEndpoint"] ?? "/api/auth/token";
    public int DefaultTimeout => int.Parse(_configuration["TestSettings:DefaultTimeout"] ?? "30000");
    public int RetryAttempts => int.Parse(_configuration["TestSettings:RetryAttempts"] ?? "3");

    public string ValidUsername => _configuration["TestSettings:ValidCredentials:Username"] ?? "TCCTest";
    public string ValidPassword => _configuration["TestSettings:ValidCredentials:Password"] ?? "Test!456";

    public string GetFullApiUrl() => $"{ApiBaseUrl}{AuthEndpoint}";

    public HttpClient CreateHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl),
            Timeout = TimeSpan.FromMilliseconds(DefaultTimeout)
        };
    }
}