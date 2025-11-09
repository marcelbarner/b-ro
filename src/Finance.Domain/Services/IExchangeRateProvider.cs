using Finance.Domain.Entities;

namespace Finance.Domain.Services;

/// <summary>
/// Provides exchange rate data from external sources.
/// </summary>
public interface IExchangeRateProvider
{
    /// <summary>
    /// Fetches exchange rates from the ECB 90-day historical feed.
    /// This feed contains the most recent exchange rates (last 90 days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of exchange rates.</returns>
    Task<IEnumerable<ExchangeRate>> Fetch90DayRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches exchange rates from the ECB full historical feed.
    /// This feed contains all available historical data since 1999.
    /// Should be used for initial import or backfilling missing data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of exchange rates.</returns>
    Task<IEnumerable<ExchangeRate>> FetchHistoricalRatesAsync(CancellationToken cancellationToken = default);
}
