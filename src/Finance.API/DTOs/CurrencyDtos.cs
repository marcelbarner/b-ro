namespace Finance.API.DTOs;

/// <summary>
/// Response DTO for currency information.
/// </summary>
public record CurrencyInfoResponse
{
    /// <summary>
    /// List of supported currency codes (ISO 4217).
    /// </summary>
    public required IEnumerable<string> SupportedCurrencies { get; init; }

    /// <summary>
    /// Timestamp when exchange rates were last updated.
    /// </summary>
    public DateTimeOffset? LastUpdated { get; init; }

    /// <summary>
    /// Total number of supported currencies.
    /// </summary>
    public int TotalCount { get; init; }
}

/// <summary>
/// Response DTO for exchange rate information.
/// </summary>
public record ExchangeRateResponse
{
    /// <summary>
    /// The date for this exchange rate.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Source currency code.
    /// </summary>
    public required string FromCurrency { get; init; }

    /// <summary>
    /// Target currency code.
    /// </summary>
    public required string ToCurrency { get; init; }

    /// <summary>
    /// The exchange rate value.
    /// </summary>
    public required decimal Rate { get; init; }
}

/// <summary>
/// Response DTO for currency conversion.
/// </summary>
public record ConversionResponse
{
    /// <summary>
    /// Original amount.
    /// </summary>
    public required decimal OriginalAmount { get; init; }

    /// <summary>
    /// Source currency code.
    /// </summary>
    public required string FromCurrency { get; init; }

    /// <summary>
    /// Converted amount.
    /// </summary>
    public required decimal ConvertedAmount { get; init; }

    /// <summary>
    /// Target currency code.
    /// </summary>
    public required string ToCurrency { get; init; }

    /// <summary>
    /// Exchange rate used for conversion.
    /// </summary>
    public required decimal ExchangeRate { get; init; }

    /// <summary>
    /// Date for which the exchange rate was retrieved.
    /// </summary>
    public required DateOnly Date { get; init; }
}
