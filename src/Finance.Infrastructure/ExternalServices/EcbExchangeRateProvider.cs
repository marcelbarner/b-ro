using System.Globalization;
using System.Xml.Linq;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure.ExternalServices;

/// <summary>
/// Provides exchange rate data from the European Central Bank (ECB) XML feeds.
/// </summary>
public class EcbExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EcbExchangeRateProvider> _logger;

    private const string Ecb90DayUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml";
    private const string EcbHistoricalUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

    // XML namespaces used by ECB
    private static readonly XNamespace GesmeNamespace = "http://www.gesmes.org/xml/2002-08-01";
    private static readonly XNamespace EcbNamespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

    public EcbExchangeRateProvider(HttpClient httpClient, ILogger<EcbExchangeRateProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ExchangeRate>> Fetch90DayRatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching 90-day exchange rates from ECB");
        return await FetchRatesAsync(Ecb90DayUrl, ExchangeRateSource.ECB90Day, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ExchangeRate>> FetchHistoricalRatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching historical exchange rates from ECB");
        return await FetchRatesAsync(EcbHistoricalUrl, ExchangeRateSource.ECBHistorical, cancellationToken);
    }

    private async Task<IEnumerable<ExchangeRate>> FetchRatesAsync(
        string url,
        ExchangeRateSource source,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch XML data
            var xmlContent = await _httpClient.GetStringAsync(url, cancellationToken);
            
            // Parse XML
            var document = XDocument.Parse(xmlContent);
            var rates = ParseExchangeRates(document, source);

            _logger.LogInformation(
                "Successfully fetched {Count} exchange rates from {Source}",
                rates.Count(),
                source);

            return rates;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching exchange rates from {Url}", url);
            throw new InvalidOperationException($"Failed to fetch exchange rates from ECB: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error while fetching exchange rates from {Url}", url);
            throw new InvalidOperationException($"Failed to parse exchange rates from ECB: {ex.Message}", ex);
        }
    }

    private IEnumerable<ExchangeRate> ParseExchangeRates(XDocument document, ExchangeRateSource source)
    {
        var rates = new List<ExchangeRate>();

        // ECB XML structure:
        // <gesmes:Envelope xmlns:gesmes="..." xmlns="...">
        //   <Cube>
        //     <Cube time='2025-11-08'>
        //       <Cube currency='USD' rate='1.0874'/>
        //       <Cube currency='GBP' rate='0.8456'/>
        //       ...
        //     </Cube>
        //   </Cube>
        // </gesmes:Envelope>

        var root = document.Root;
        if (root == null)
        {
            _logger.LogWarning("XML document has no root element");
            return rates;
        }

        // Navigate to the outer Cube element
        var outerCube = root.Element(EcbNamespace + "Cube");
        if (outerCube == null)
        {
            _logger.LogWarning("No Cube element found in XML");
            return rates;
        }

        // Iterate through date Cube elements
        foreach (var dateCube in outerCube.Elements(EcbNamespace + "Cube"))
        {
            var timeAttribute = dateCube.Attribute("time");
            if (timeAttribute == null)
            {
                _logger.LogWarning("Cube element missing 'time' attribute");
                continue;
            }

            if (!DateOnly.TryParseExact(
                timeAttribute.Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                _logger.LogWarning("Invalid date format: {Date}", timeAttribute.Value);
                continue;
            }

            // Iterate through currency Cube elements
            foreach (var currencyCube in dateCube.Elements(EcbNamespace + "Cube"))
            {
                var currencyAttribute = currencyCube.Attribute("currency");
                var rateAttribute = currencyCube.Attribute("rate");

                if (currencyAttribute == null || rateAttribute == null)
                {
                    _logger.LogWarning("Cube element missing 'currency' or 'rate' attribute");
                    continue;
                }

                if (!decimal.TryParse(
                    rateAttribute.Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var rate))
                {
                    _logger.LogWarning(
                        "Invalid rate format for {Currency}: {Rate}",
                        currencyAttribute.Value,
                        rateAttribute.Value);
                    continue;
                }

                try
                {
                    var exchangeRate = new ExchangeRate(
                        date,
                        currencyAttribute.Value,
                        rate,
                        source);

                    rates.Add(exchangeRate);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to create ExchangeRate for {Currency} on {Date}",
                        currencyAttribute.Value,
                        date);
                }
            }
        }

        return rates;
    }
}
