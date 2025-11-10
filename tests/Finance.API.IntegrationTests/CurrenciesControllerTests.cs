using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Finance.API.IntegrationTests;

/// <summary>
/// Integration tests for the Currencies API endpoints.
/// Tests currency conversion and exchange rate retrieval.
/// </summary>
public class CurrenciesControllerTests : IClassFixture<AspireAppFixture>
{
    private readonly HttpClient _httpClient;

    public CurrenciesControllerTests(AspireAppFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task ConvertCurrency_ValidConversion_ReturnsOk()
    {
        // Arrange
        var fromCurrency = "EUR";
        var toCurrency = "USD";
        var amount = 100.00m;

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/convert?from={fromCurrency}&to={toCurrency}&amount={amount}"
        );

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CurrencyConversionResult>();
        result.Should().NotBeNull();
        result!.FromCurrency.Should().Be(fromCurrency);
        result.ToCurrency.Should().Be(toCurrency);
        result.OriginalAmount.Should().Be(amount);
        result.ConvertedAmount.Should().BeGreaterThan(0);
        result.ExchangeRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertCurrency_SameCurrency_ReturnsOriginalAmount()
    {
        // Arrange
        var currency = "EUR";
        var amount = 250.00m;

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/convert?from={currency}&to={currency}&amount={amount}"
        );

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<CurrencyConversionResult>();
        result.Should().NotBeNull();
        result!.ConvertedAmount.Should().Be(amount);
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    public async Task ConvertCurrency_InvalidCurrency_ReturnsBadRequest()
    {
        // Arrange
        var fromCurrency = "XXX"; // Invalid currency
        var toCurrency = "USD";
        var amount = 100.00m;

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/convert?from={fromCurrency}&to={toCurrency}&amount={amount}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConvertCurrency_NegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var amount = -50.00m;

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/convert?from=EUR&to=USD&amount={amount}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("EUR", "USD")]
    [InlineData("GBP", "EUR")]
    [InlineData("USD", "JPY")]
    [InlineData("CHF", "EUR")]
    public async Task ConvertCurrency_VariousCurrencyPairs_ReturnsValidConversion(string from, string to)
    {
        // Arrange
        var amount = 100.00m;

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/convert?from={from}&to={to}&amount={amount}"
        );

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<CurrencyConversionResult>();
        result.Should().NotBeNull();
        result!.ConvertedAmount.Should().BeGreaterThan(0);
        result.ExchangeRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetExchangeRate_ValidPair_ReturnsRate()
    {
        // Arrange
        var fromCurrency = "EUR";
        var toCurrency = "USD";

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/rate?from={fromCurrency}&to={toCurrency}"
        );

        // Assert
        response.Should().BeSuccessful();
        var rate = await response.Content.ReadFromJsonAsync<decimal>();
        rate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetExchangeRate_WithSpecificDate_ReturnsHistoricalRate()
    {
        // Arrange
        var fromCurrency = "EUR";
        var toCurrency = "USD";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)); // One week ago

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/finance/currencies/rate?from={fromCurrency}&to={toCurrency}&date={date:yyyy-MM-dd}"
        );

        // Assert
        response.Should().BeSuccessful();
        var rate = await response.Content.ReadFromJsonAsync<decimal>();
        rate.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Helper record for deserializing currency conversion results.
    /// </summary>
    private record CurrencyConversionResult(
        string FromCurrency,
        string ToCurrency,
        decimal OriginalAmount,
        decimal ConvertedAmount,
        decimal ExchangeRate,
        DateOnly RateDate
    );
}
