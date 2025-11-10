using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BRo.E2E.Tests;

/// <summary>
/// End-to-end tests for the Finance application frontend.
/// </summary>
public class FinanceAppTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    private const string BaseUrl = "http://localhost:4200";

    public FinanceAppTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HomePage_ShouldLoad()
    {
        // Arrange
        var page = _fixture.Page!;

        // Act
        await page.GotoAsync(BaseUrl);

        // Assert
        var title = await page.TitleAsync();
        Assert.NotNull(title);
    }

    [Fact]
    public async Task AccountsPage_ShouldDisplayAccounts()
    {
        // Arrange
        var page = _fixture.Page!;

        // Act
        await page.GotoAsync($"{BaseUrl}/accounts");
        await page.WaitForLoadStateAsync();

        // Assert
        var accountsSection = page.Locator("app-account-list");
        await Expect(accountsSection).ToBeVisibleAsync();
    }

    [Fact]
    public async Task TransactionsPage_ShouldLoad()
    {
        // Arrange
        var page = _fixture.Page!;

        // Act
        await page.GotoAsync($"{BaseUrl}/transactions");
        await page.WaitForLoadStateAsync();

        // Assert
        var transactionsSection = page.Locator("app-transaction-list");
        await Expect(transactionsSection).ToBeVisibleAsync();
    }
}
