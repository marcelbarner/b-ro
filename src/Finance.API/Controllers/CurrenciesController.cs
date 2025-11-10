using Finance.API.DTOs;
using Finance.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers;

/// <summary>
/// API endpoints for currency management and exchange rate operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrenciesController> _logger;

    public CurrenciesController(
        ICurrencyService currencyService,
        ILogger<CurrenciesController> logger)
    {
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets list of supported currencies and last update time.
    /// </summary>
    /// <response code="200">Returns supported currencies</response>
    [HttpGet]
    [ProducesResponseType(typeof(CurrencyInfoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrencyInfoResponse>> GetCurrencies(CancellationToken cancellationToken)
    {
        var currencies = await _currencyService.GetSupportedCurrenciesAsync(cancellationToken);
        var lastUpdated = await _currencyService.GetLastUpdateTimeAsync(cancellationToken);

        var response = new CurrencyInfoResponse
        {
            SupportedCurrencies = currencies,
            LastUpdated = lastUpdated,
            TotalCount = currencies.Count()
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the exchange rate between two currencies for a specific date.
    /// </summary>
    /// <param name="from">Source currency code (ISO 4217)</param>
    /// <param name="to">Target currency code (ISO 4217)</param>
    /// <param name="date">Date for the exchange rate (optional, defaults to latest)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns the exchange rate</response>
    /// <response code="404">Exchange rate not found for the specified date and currency pair</response>
    /// <response code="400">Invalid currency codes or date</response>
    [HttpGet("exchange-rate")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExchangeRateResponse>> GetExchangeRate(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return BadRequest("Both 'from' and 'to' currency codes are required");
        }

        var rateDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rate = await _currencyService.GetRateAsync(rateDate, from, to, cancellationToken);

        if (!rate.HasValue)
        {
            _logger.LogWarning("Exchange rate not found: {From} -> {To} on {Date}", from, to, rateDate);
            return NotFound($"Exchange rate not found for {from} to {to} on {rateDate}");
        }

        var response = new ExchangeRateResponse
        {
            Date = rateDate,
            FromCurrency = from.ToUpperInvariant(),
            ToCurrency = to.ToUpperInvariant(),
            Rate = rate.Value
        };

        return Ok(response);
    }

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="from">Source currency code (ISO 4217)</param>
    /// <param name="to">Target currency code (ISO 4217)</param>
    /// <param name="date">Date for the exchange rate (optional, defaults to latest)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns the converted amount</response>
    /// <response code="404">Exchange rate not found for the specified date and currency pair</response>
    /// <response code="400">Invalid parameters</response>
    [HttpGet("convert")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConversionResponse>> ConvertCurrency(
        [FromQuery] decimal amount,
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        if (amount < 0)
        {
            return BadRequest("Amount cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return BadRequest("Both 'from' and 'to' currency codes are required");
        }

        var conversionDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var convertedAmount = await _currencyService.ConvertAmountAsync(
            amount,
            from,
            to,
            conversionDate,
            cancellationToken);

        if (!convertedAmount.HasValue)
        {
            _logger.LogWarning("Cannot convert: exchange rate not found for {From} -> {To} on {Date}",
                from, to, conversionDate);
            return NotFound($"Exchange rate not found for {from} to {to} on {conversionDate}");
        }

        var rate = await _currencyService.GetRateAsync(conversionDate, from, to, cancellationToken);

        var response = new ConversionResponse
        {
            OriginalAmount = amount,
            FromCurrency = from.ToUpperInvariant(),
            ConvertedAmount = convertedAmount.Value,
            ToCurrency = to.ToUpperInvariant(),
            ExchangeRate = rate!.Value,
            Date = conversionDate
        };

        return Ok(response);
    }
}
