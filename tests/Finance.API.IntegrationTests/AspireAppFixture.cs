using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Finance.API.IntegrationTests;

/// <summary>
/// Base fixture for integration tests using Aspire distributed application testing.
/// Provides a running instance of the entire application stack including PostgreSQL database.
/// </summary>
public class AspireAppFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _httpClient;

    /// <summary>
    /// Gets the HTTP client configured to communicate with the Finance API.
    /// </summary>
    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("Fixture not initialized. Ensure InitializeAsync was called.");

    /// <summary>
    /// Gets the distributed application instance for advanced scenarios.
    /// </summary>
    public DistributedApplication App => _app ?? throw new InvalidOperationException("Fixture not initialized. Ensure InitializeAsync was called.");

    /// <summary>
    /// Initializes the Aspire distributed application and creates an HTTP client.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Build the Aspire distributed application
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.BRo_AppHost>();

        // Start the application (includes PostgreSQL database, Finance API, frontend)
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Create HTTP client for the Finance API
        // In CI environments (Linux), we may need to skip SSL certificate validation
        var httpClient = _app.CreateHttpClient("finance-api");
        
        // Configure to accept any SSL certificate in test environments
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = httpClient.BaseAddress,
            Timeout = httpClient.Timeout
        };
        
        // Copy default request headers
        foreach (var header in httpClient.DefaultRequestHeaders)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        
        httpClient.Dispose();
    }

    /// <summary>
    /// Disposes the distributed application and HTTP client.
    /// </summary>
    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}
