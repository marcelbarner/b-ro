namespace Finance.Domain.Enums;

/// <summary>
/// Defines the source from which exchange rate data was obtained.
/// </summary>
public enum ExchangeRateSource
{
    /// <summary>
    /// Data from ECB 90-day historical feed.
    /// Updated daily with recent exchange rates.
    /// </summary>
    ECB90Day = 1,

    /// <summary>
    /// Data from ECB full historical feed.
    /// Used for backfilling missing data or initial import.
    /// </summary>
    ECBHistorical = 2
}
