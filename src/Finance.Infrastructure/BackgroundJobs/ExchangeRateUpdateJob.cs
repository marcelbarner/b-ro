using Finance.Domain.Services;
using Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that automatically updates exchange rates daily from ECB.
/// Runs at 03:00 UTC daily (after ECB publishes rates around 16:00 CET).
/// </summary>
public class ExchangeRateUpdateJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExchangeRateUpdateJob> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(24);
    private readonly TimeOnly _updateTime = new(3, 0); // 03:00 UTC

    public ExchangeRateUpdateJob(
        IServiceProvider serviceProvider,
        ILogger<ExchangeRateUpdateJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exchange Rate Update Job started");

        // Run immediately on startup (for initial data load or catching up)
        await UpdateExchangeRatesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate time until next update
                var nextUpdate = CalculateNextUpdateTime();
                var delay = nextUpdate - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "Next exchange rate update scheduled for {NextUpdate} (in {Hours:F1} hours)",
                        nextUpdate,
                        delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await UpdateExchangeRatesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Exchange Rate Update Job is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Exchange Rate Update Job");
                
                // Wait a bit before retrying to avoid tight loop on persistent errors
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Exchange Rate Update Job stopped");
    }

    private async Task UpdateExchangeRatesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting exchange rate update...");
        var startTime = DateTime.UtcNow;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<IExchangeRateProvider>();
            var context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

            // Fetch 90-day rates from ECB
            var rates = await FetchRatesWithRetryAsync(
                () => provider.Fetch90DayRatesAsync(cancellationToken),
                "90-day feed",
                cancellationToken);

            if (rates == null || !rates.Any())
            {
                _logger.LogWarning("No rates retrieved from 90-day feed");
                return;
            }

            // Detect gaps and backfill if needed
            await DetectAndBackfillGapsAsync(context, provider, cancellationToken);

            // Save new rates
            var savedCount = await SaveRatesAsync(context, rates, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Exchange rate update completed successfully. Saved {Count} rates in {Duration:F1} seconds",
                savedCount,
                duration.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update exchange rates");
            throw;
        }
    }

    private async Task<IEnumerable<Finance.Domain.Entities.ExchangeRate>?> FetchRatesWithRetryAsync(
        Func<Task<IEnumerable<Finance.Domain.Entities.ExchangeRate>>> fetchAction,
        string source,
        CancellationToken cancellationToken,
        int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Fetching rates from {Source} (attempt {Attempt}/{Max})",
                    source, attempt, maxRetries);

                return await fetchAction();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                _logger.LogWarning(ex,
                    "Failed to fetch rates from {Source} (attempt {Attempt}/{Max}). Retrying in {Delay} seconds",
                    source, attempt, maxRetries, delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        _logger.LogError("Failed to fetch rates from {Source} after {Max} attempts", source, maxRetries);
        return null;
    }

    private async Task DetectAndBackfillGapsAsync(
        FinanceDbContext context,
        IExchangeRateProvider provider,
        CancellationToken cancellationToken)
    {
        // Get the date range we should have
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ninetyDaysAgo = today.AddDays(-90);

        // Get dates we actually have
        var existingDates = await context.ExchangeRates
            .Where(e => e.Date >= ninetyDaysAgo && e.Date <= today)
            .Select(e => e.Date)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Detect missing dates (excluding weekends - ECB doesn't publish on weekends)
        var missingDates = new List<DateOnly>();
        for (var date = ninetyDaysAgo; date <= today; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
            {
                if (!existingDates.Contains(date))
                {
                    missingDates.Add(date);
                }
            }
        }

        if (missingDates.Any())
        {
            _logger.LogWarning("Detected {Count} missing dates in exchange rate data. Backfilling from historical feed...",
                missingDates.Count);

            // Fetch full historical data
            var historicalRates = await FetchRatesWithRetryAsync(
                () => provider.FetchHistoricalRatesAsync(cancellationToken),
                "historical feed",
                cancellationToken);

            if (historicalRates != null && historicalRates.Any())
            {
                // Filter to only missing dates
                var backfillRates = historicalRates
                    .Where(r => missingDates.Contains(r.Date))
                    .ToList();

                if (backfillRates.Any())
                {
                    var backfilledCount = await SaveRatesAsync(context, backfillRates, cancellationToken);
                    _logger.LogInformation("Backfilled {Count} missing dates", backfilledCount);
                }
            }
        }
        else
        {
            _logger.LogDebug("No gaps detected in exchange rate data");
        }
    }

    private async Task<int> SaveRatesAsync(
        FinanceDbContext context,
        IEnumerable<Finance.Domain.Entities.ExchangeRate> rates,
        CancellationToken cancellationToken)
    {
        var savedCount = 0;

        foreach (var rate in rates)
        {
            // Check if rate already exists
            var existing = await context.ExchangeRates
                .FirstOrDefaultAsync(
                    e => e.Date == rate.Date && e.TargetCurrency == rate.TargetCurrency,
                    cancellationToken);

            if (existing != null)
            {
                // Update existing rate
                existing.UpdateRate(rate.Rate, rate.Source);
                _logger.LogDebug("Updated rate for {Currency} on {Date}: {Rate}",
                    rate.TargetCurrency, rate.Date, rate.Rate);
            }
            else
            {
                // Add new rate
                context.ExchangeRates.Add(rate);
                savedCount++;
                _logger.LogDebug("Added new rate for {Currency} on {Date}: {Rate}",
                    rate.TargetCurrency, rate.Date, rate.Rate);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return savedCount;
    }

    private DateTime CalculateNextUpdateTime()
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var todayUpdate = today.ToDateTime(_updateTime, DateTimeKind.Utc);

        if (now < todayUpdate)
        {
            return todayUpdate;
        }
        else
        {
            // Schedule for tomorrow
            return today.AddDays(1).ToDateTime(_updateTime, DateTimeKind.Utc);
        }
    }
}
