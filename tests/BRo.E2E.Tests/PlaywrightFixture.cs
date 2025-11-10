using Microsoft.Playwright;

namespace BRo.E2E.Tests;

/// <summary>
/// Playwright fixture for E2E tests providing browser context and page instances.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowserContext? Context { get; private set; }
    public IPage? Page { get; private set; }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
            await Page.CloseAsync();
        
        if (Context != null)
            await Context.CloseAsync();
        
        if (_browser != null)
            await _browser.CloseAsync();
        
        _playwright?.Dispose();
    }
}
