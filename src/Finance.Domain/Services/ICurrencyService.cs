using Finance.Domain.Entities;

namespace Finance.Domain.Services;

/// <summary>
/// Provides currency conversion and exchange rate services.
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Gets the exchange rate for a specific date and currency pair.
    /// </summary>
    /// <param name="date">The date for which to get the rate.</param>
    /// <param name="fromCurrency">Source currency code (ISO 4217).</param>
    /// <param name="toCurrency">Target currency code (ISO 4217).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchange rate, or null if not found.</returns>
    Task<decimal?> GetRateAsync(
        DateOnly date,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest available exchange rates for all currencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of the most recent exchange rates.</returns>
    Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all supported currency codes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of ISO 4217 currency codes.</returns>
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts an amount from one currency to another.
    /// Uses the exchange rate for the specified date, or the latest rate if no date is provided.
    /// </summary>
    /// <param name="amount">The amount to convert.</param>
    /// <param name="fromCurrency">Source currency code (ISO 4217).</param>
    /// <param name="toCurrency">Target currency code (ISO 4217).</param>
    /// <param name="date">Optional date for historical conversion. Uses latest rate if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The converted amount, or null if rate not available.</returns>
    Task<decimal?> ConvertAmountAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        DateOnly? date = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the date and time when exchange rates were last updated.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The timestamp of the last update, or null if no rates exist.</returns>
    Task<DateTimeOffset?> GetLastUpdateTimeAsync(CancellationToken cancellationToken = default);
}
