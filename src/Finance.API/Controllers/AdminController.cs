using Finance.Domain.Services;
using Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.API.Controllers;

/// <summary>
/// Administrative endpoints for debugging and maintenance.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly FinanceDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IExchangeRateProvider exchangeRateProvider,
        FinanceDbContext context,
        ILogger<AdminController> logger)
    {
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Manually trigger exchange rate fetch and seed.
    /// </summary>
    [HttpPost("seed-exchange-rates")]
    public async Task<IActionResult> SeedExchangeRates(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Manual exchange rate seed triggered");

            var rates = await _exchangeRateProvider.Fetch90DayRatesAsync(cancellationToken);

            if (rates == null || !rates.Any())
            {
                return BadRequest("Failed to fetch exchange rates from ECB");
            }

            // Clear existing rates
            _context.ExchangeRates.RemoveRange(_context.ExchangeRates);

            // Add new rates
            await _context.ExchangeRates.AddRangeAsync(rates, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Seeded {Count} exchange rates", rates.Count());

            return Ok(new
            {
                message = "Exchange rates seeded successfully",
                count = rates.Count(),
                latestDate = rates.Max(r => r.Date),
                currencies = rates.Select(r => r.TargetCurrency).Distinct().Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed exchange rates");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get database statistics.
    /// </summary>
    [HttpGet("db-stats")]
    public async Task<IActionResult> GetDatabaseStats(CancellationToken cancellationToken)
    {
        var stats = new
        {
            exchangeRatesCount = await _context.ExchangeRates.CountAsync(cancellationToken),
            accountsCount = await _context.Accounts.CountAsync(cancellationToken),
            transactionsCount = await _context.Transactions.CountAsync(cancellationToken),
            latestExchangeRateDate = await _context.ExchangeRates
                .MaxAsync(e => (DateOnly?)e.Date, cancellationToken),
            currencies = await _context.ExchangeRates
                .Select(e => e.TargetCurrency)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(cancellationToken)
        };

        return Ok(stats);
    }
}
