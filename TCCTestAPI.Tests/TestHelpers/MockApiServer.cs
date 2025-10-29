using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net;
using TCCTestAPI.Tests.Models;
using System.Text.Json;

namespace TCCTestAPI.Tests.TestHelpers;

/// <summary>
/// WireMock server setup for API testing
/// </summary>
public class MockApiServer : IDisposable
{
    private WireMockServer? _server;

    public string BaseUrl => _server?.Urls[0] ?? throw new InvalidOperationException("Server not started");

    public void Start(int port = 0)
    {
        var settings = new WireMockServerSettings
        {
            Port = port == 0 ? null : port,
            StartAdminInterface = true,
            AllowPartialMapping = true
        };

        _server = WireMockServer.Start(settings);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        if (_server == null) return;

        // Mock successful authentication
        _server
            .Given(
                Request.Create()
                    .WithPath("/api/auth/token")
                    .UsingPost()
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new { username = "TCCTest", password = "Test!456" }))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new AuthenticationResponse
                    {
                        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IlRDQ1Rlc3QiLCJuYW1laWQiOiJUQ0NUZXN0Iiwic3ViIjoiVENDVGVzdCIsImp0aSI6IjM4OGZiODRkLWQzY2QtNGYxOS04MzBjLWU5MzEyY2NlMjliNSIsImlhdCI6MTc2MTc1MDUxMywibmJmIjoxNzYxNzUwNTEzLCJleHAiOjE3NjE3NTQxMTMsImlzcyI6IlRDQ0p3dEFwaSIsImF1ZCI6IlRDQ0p3dEFwaVVzZXJzIn0.JU_onHu05CJwaW5YfrBQDs3IdXSgur4enWeKPpj6mzI",
                        Expires = DateTime.UtcNow.AddHours(1),
                        Username = "TCCTest",
                        Message = "Welcome Admin: TCCTest"
                    }));

        // Mock failed authentication
        _server
            .Given(
                Request.Create()
                    .WithPath("/api/auth/token")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.Unauthorized)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new { message = "Invalid credentials" }));

        // Mock bad request for invalid JSON
        _server
            .Given(
                Request.Create()
                    .WithPath("/api/auth/token")
                    .UsingPost()
                    .WithBody("{ invalid json }"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.BadRequest)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new { message = "Invalid JSON format" }));
    }

    public void Reset()
    {
        _server?.Reset();
        SetupDefaultMocks();
    }

    public void Stop()
    {
        _server?.Stop();
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}