using Finance.Domain.Enums;

namespace Finance.Domain.Entities;

/// <summary>
/// Represents an exchange rate between currencies on a specific date.
/// Exchange rates are sourced from the European Central Bank (ECB).
/// All rates use EUR as the base currency.
/// </summary>
public class ExchangeRate
{
    /// <summary>
    /// Unique identifier for the exchange rate record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The date for which this exchange rate is valid.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// The base currency code (ISO 4217).
    /// ECB uses EUR as base currency, so this will always be "EUR".
    /// </summary>
    public string BaseCurrency { get; private set; } = "EUR";

    /// <summary>
    /// The target currency code (ISO 4217).
    /// </summary>
    public string TargetCurrency { get; private set; } = string.Empty;

    /// <summary>
    /// The exchange rate value.
    /// For example, if 1 EUR = 1.0874 USD, the rate is 1.0874.
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    /// The source from which this exchange rate data was obtained.
    /// </summary>
    public ExchangeRateSource Source { get; private set; }

    /// <summary>
    /// Timestamp when this record was created in the database.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private ExchangeRate() { }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new exchange rate record.
    /// </summary>
    /// <param name="date">The date for this exchange rate.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <param name="rate">The exchange rate value.</param>
    /// <param name="source">The data source.</param>
    public ExchangeRate(DateOnly date, string targetCurrency, decimal rate, ExchangeRateSource source)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
            throw new ArgumentException("Target currency cannot be empty.", nameof(targetCurrency));
        
        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(rate));

        Id = Guid.NewGuid();
        Date = date;
        BaseCurrency = "EUR";
        TargetCurrency = targetCurrency.ToUpperInvariant();
        Rate = rate;
        Source = source;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the exchange rate value.
    /// Used when refreshing data from ECB.
    /// </summary>
    public void UpdateRate(decimal newRate, ExchangeRateSource source)
    {
        if (newRate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(newRate));

        Rate = newRate;
        Source = source;
    }
}
