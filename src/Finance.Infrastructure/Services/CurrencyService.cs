using Finance.Domain.Entities;
using Finance.Domain.Services;
using Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure.Services;

/// <summary>
/// Provides currency conversion and exchange rate services with caching.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly FinanceDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyService> _logger;

    private const string CachePrefixLatestRates = "latest_rates";
    private const string CachePrefixSupportedCurrencies = "supported_currencies";
    private const string CachePrefixRate = "rate";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public CurrencyService(
        FinanceDbContext context,
        IMemoryCache cache,
        ILogger<CurrencyService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<decimal?> GetRateAsync(
        DateOnly date,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
            throw new ArgumentException("From currency cannot be empty.", nameof(fromCurrency));

        if (string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("To currency cannot be empty.", nameof(toCurrency));

        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        // Same currency = rate is 1
        if (fromCurrency == toCurrency)
            return 1.0m;

        // Check cache first
        var cacheKey = $"{CachePrefixRate}:{date}:{fromCurrency}:{toCurrency}";
        if (_cache.TryGetValue<decimal>(cacheKey, out var cachedRate))
        {
            _logger.LogDebug("Cache hit for rate {From} -> {To} on {Date}", fromCurrency, toCurrency, date);
            return cachedRate;
        }

        decimal? rate = null;

        // If one currency is EUR, direct lookup
        if (fromCurrency == "EUR")
        {
            rate = await GetDirectRateAsync(date, toCurrency, cancellationToken);
        }
        else if (toCurrency == "EUR")
        {
            // Inverse rate: EUR/USD = 1.0874 means USD/EUR = 1/1.0874
            var directRate = await GetDirectRateAsync(date, fromCurrency, cancellationToken);
            rate = directRate.HasValue ? 1.0m / directRate.Value : null;
        }
        else
        {
            // Triangular arbitrage: USD/GBP = (USD/EUR) × (EUR/GBP)
            var fromToEur = await GetDirectRateAsync(date, fromCurrency, cancellationToken);
            var eurToTarget = await GetDirectRateAsync(date, toCurrency, cancellationToken);

            if (fromToEur.HasValue && eurToTarget.HasValue)
            {
                // fromCurrency to EUR, then EUR to toCurrency
                rate = (1.0m / fromToEur.Value) * eurToTarget.Value;
            }
        }

        // Cache the result
        if (rate.HasValue)
        {
            _cache.Set(cacheKey, rate.Value, CacheDuration);
            _logger.LogDebug("Calculated and cached rate {From} -> {To} on {Date}: {Rate}",
                fromCurrency, toCurrency, date, rate.Value);
        }
        else
        {
            _logger.LogWarning("No exchange rate found for {From} -> {To} on {Date}",
                fromCurrency, toCurrency, date);
        }

        return rate;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache
        if (_cache.TryGetValue<IEnumerable<ExchangeRate>>(CachePrefixLatestRates, out var cached))
        {
            _logger.LogDebug("Cache hit for latest rates");
            return cached!;
        }

        // Get the latest date with rates
        var latestDate = await _context.ExchangeRates
            .MaxAsync(e => (DateOnly?)e.Date, cancellationToken);

        if (!latestDate.HasValue)
        {
            _logger.LogWarning("No exchange rates found in database");
            return Enumerable.Empty<ExchangeRate>();
        }

        var rates = await _context.ExchangeRates
            .Where(e => e.Date == latestDate.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Cache for 1 hour
        _cache.Set(CachePrefixLatestRates, rates, CacheDuration);

        _logger.LogInformation("Retrieved {Count} latest exchange rates for {Date}",
            rates.Count, latestDate.Value);

        return rates;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache
        if (_cache.TryGetValue<IEnumerable<string>>(CachePrefixSupportedCurrencies, out var cached))
        {
            _logger.LogDebug("Cache hit for supported currencies");
            return cached!;
        }

        var currencies = await _context.ExchangeRates
            .Select(e => e.TargetCurrency)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        // EUR is always supported (base currency)
        if (!currencies.Contains("EUR"))
        {
            currencies.Insert(0, "EUR");
        }

        // Cache for 1 hour
        _cache.Set(CachePrefixSupportedCurrencies, currencies, CacheDuration);

        _logger.LogInformation("Retrieved {Count} supported currencies", currencies.Count);

        return currencies;
    }

    /// <inheritdoc/>
    public async Task<decimal?> ConvertAmountAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        // Use latest available rate if no date specified
        DateOnly conversionDate;
        if (date.HasValue)
        {
            conversionDate = date.Value;
        }
        else
        {
            // Get the most recent date with rates (ECB doesn't publish on weekends)
            var latestDate = await _context.ExchangeRates
                .MaxAsync(e => (DateOnly?)e.Date, cancellationToken);

            conversionDate = latestDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        }

        var rate = await GetRateAsync(conversionDate, fromCurrency, toCurrency, cancellationToken);

        if (!rate.HasValue)
            return null;

        var convertedAmount = amount * rate.Value;

        _logger.LogDebug("Converted {Amount} {From} to {Result} {To} using rate {Rate}",
            amount, fromCurrency, convertedAmount, toCurrency, rate.Value);

        return convertedAmount;
    }

    /// <inheritdoc/>
    public async Task<DateTimeOffset?> GetLastUpdateTimeAsync(CancellationToken cancellationToken = default)
    {
        var lastUpdate = await _context.ExchangeRates
            .MaxAsync(e => (DateTimeOffset?)e.CreatedAt, cancellationToken);

        _logger.LogDebug("Last exchange rate update: {Time}", lastUpdate);

        return lastUpdate;
    }

    /// <summary>
    /// Gets a direct exchange rate from EUR to the target currency.
    /// </summary>
    private async Task<decimal?> GetDirectRateAsync(
        DateOnly date,
        string targetCurrency,
        CancellationToken cancellationToken)
    {
        var rate = await _context.ExchangeRates
            .Where(e => e.Date == date
                && e.BaseCurrency == "EUR"
                && e.TargetCurrency == targetCurrency)
            .Select(e => (decimal?)e.Rate)
            .FirstOrDefaultAsync(cancellationToken);

        return rate;
    }
}
