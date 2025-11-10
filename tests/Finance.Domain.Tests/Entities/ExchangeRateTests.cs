using Finance.Domain.Entities;
using Finance.Domain.Enums;

namespace Finance.Domain.Tests.Entities;

public class ExchangeRateTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesExchangeRate()
    {
        // Arrange
        var date = new DateOnly(2025, 11, 8);
        var targetCurrency = "USD";
        var rate = 1.0874m;
        var source = ExchangeRateSource.ECB90Day;

        // Act
        var exchangeRate = new ExchangeRate(date, targetCurrency, rate, source);

        // Assert
        Assert.NotEqual(Guid.Empty, exchangeRate.Id);
        Assert.Equal(date, exchangeRate.Date);
        Assert.Equal("EUR", exchangeRate.BaseCurrency);
        Assert.Equal("USD", exchangeRate.TargetCurrency);
        Assert.Equal(rate, exchangeRate.Rate);
        Assert.Equal(source, exchangeRate.Source);
        Assert.True(exchangeRate.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_WithLowercaseCurrency_ConvertsToUppercase()
    {
        // Arrange
        var date = new DateOnly(2025, 11, 8);
        var targetCurrency = "usd";

        // Act
        var exchangeRate = new ExchangeRate(date, targetCurrency, 1.0874m, ExchangeRateSource.ECB90Day);

        // Assert
        Assert.Equal("USD", exchangeRate.TargetCurrency);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCurrency_ThrowsArgumentException(string? currency)
    {
        // Arrange
        var date = new DateOnly(2025, 11, 8);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ExchangeRate(date, currency!, 1.0874m, ExchangeRateSource.ECB90Day));

        Assert.Equal("targetCurrency", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.5)]
    public void Constructor_WithNonPositiveRate_ThrowsArgumentException(decimal rate)
    {
        // Arrange
        var date = new DateOnly(2025, 11, 8);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ExchangeRate(date, "USD", rate, ExchangeRateSource.ECB90Day));

        Assert.Equal("rate", exception.ParamName);
    }

    [Fact]
    public void UpdateRate_WithValidRate_UpdatesRateAndSource()
    {
        // Arrange
        var exchangeRate = new ExchangeRate(
            new DateOnly(2025, 11, 8),
            "USD",
            1.0874m,
            ExchangeRateSource.ECB90Day);

        var newRate = 1.0950m;
        var newSource = ExchangeRateSource.ECBHistorical;

        // Act
        exchangeRate.UpdateRate(newRate, newSource);

        // Assert
        Assert.Equal(newRate, exchangeRate.Rate);
        Assert.Equal(newSource, exchangeRate.Source);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateRate_WithNonPositiveRate_ThrowsArgumentException(decimal rate)
    {
        // Arrange
        var exchangeRate = new ExchangeRate(
            new DateOnly(2025, 11, 8),
            "USD",
            1.0874m,
            ExchangeRateSource.ECB90Day);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            exchangeRate.UpdateRate(rate, ExchangeRateSource.ECBHistorical));

        Assert.Equal("newRate", exception.ParamName);
    }
}
