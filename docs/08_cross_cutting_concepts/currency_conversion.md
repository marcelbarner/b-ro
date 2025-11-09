# Currency Conversion

## Overview
The Finance application supports multi-currency accounts and transactions with automatic exchange rate management via the European Central Bank (ECB). This cross-cutting concern affects data persistence, business logic, and API responses.

## Strategy

### Base Currency
- **EUR (Euro)** is the base currency for all exchange rate storage
- All ECB rates are published relative to EUR
- All conversions use EUR as an intermediary

### Exchange Rate Sources
1. **Primary**: ECB 90-Day Feed
   - URL: `https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml`
   - Updated: Daily around 16:00 CET
   - Coverage: Last 90 calendar days

2. **Secondary**: ECB Historical Feed
   - URL: `https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml`
   - Coverage: Complete history since 1999
   - Used only for gap-filling

### Supported Currencies
Approximately 40 currencies including:
- **Major**: USD, GBP, CHF, JPY, CNY, CAD, AUD
- **European**: SEK, NOK, DKK, PLN, CZK, HUF, RON, BGN, HRK
- **Asian**: INR, KRW, SGD, HKD, THB, PHP, MYR, IDR
- **Others**: BRL, ZAR, MXN, NZD, TRY, RUB, ISK

## Implementation

### Database Schema

#### ExchangeRates Table
```sql
CREATE TABLE ExchangeRates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Date DATE NOT NULL,
    BaseCurrency NVARCHAR(3) NOT NULL DEFAULT 'EUR',
    TargetCurrency NVARCHAR(3) NOT NULL,
    Rate DECIMAL(18,6) NOT NULL,
    Source NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UNIQUE (Date, TargetCurrency)
);

CREATE INDEX IX_ExchangeRates_Date_TargetCurrency 
ON ExchangeRates (Date, TargetCurrency);
```

#### Transactions Table Extension
```sql
ALTER TABLE Transactions 
ADD ExchangeRateToEUR DECIMAL(18,6) NULL;
```

- **Purpose**: Store exchange rate at transaction creation time
- **Nullable**: Yes (NULL for EUR transactions, or pre-existing transactions)
- **Usage**: Enables accurate historical reporting
- **Computed**: `ConvertedAmountEUR = Amount * ExchangeRateToEUR`

### Conversion Logic

#### Direct EUR Conversion
```csharp
// EUR → XXX (use ECB rate directly)
var rate = await GetRateAsync(date, "EUR", "USD");
var result = amountEUR * rate;

// XXX → EUR (inverse ECB rate)
var rate = await GetRateAsync(date, "EUR", "USD");
var result = amountUSD / rate;
```

#### Triangular Arbitrage
For non-EUR pairs, convert through EUR:
```csharp
// USD → GBP
var eurToUsd = await GetRateAsync(date, "EUR", "USD");
var eurToGbp = await GetRateAsync(date, "EUR", "GBP");
var usdToGbp = eurToGbp / eurToUsd;
var result = amountUSD * usdToGbp;
```

**Formula**: `Rate(A → B) = Rate(EUR → B) / Rate(EUR → A)`

#### Same Currency
```csharp
if (fromCurrency == toCurrency) 
{
    return amount; // No conversion needed
}
```

### Caching Strategy

#### Cache Configuration
- **Provider**: `IMemoryCache` (in-memory)
- **TTL**: 1 hour (sliding expiration)
- **Eviction**: Automatic (no manual clearing)

#### Cache Keys
```csharp
const string CachePrefixRate = "exchange_rate";
const string CachePrefixLatestRates = "latest_rates";
const string CachePrefixSupportedCurrencies = "supported_currencies";

// Examples:
// "exchange_rate_usd_eur_2025-11-09"
// "latest_rates"
// "supported_currencies"
```

#### Cache Patterns
```csharp
// Check cache first
if (_cache.TryGetValue(cacheKey, out decimal? cachedRate))
{
    return cachedRate;
}

// Fetch from database
var rate = await FetchFromDatabaseAsync(...);

// Store in cache
_cache.Set(cacheKey, rate, TimeSpan.FromHours(1));

return rate;
```

### Background Job

#### Schedule
- **Frequency**: Daily
- **Time**: 03:00 UTC
- **Timezone**: UTC (independent of server location)
- **Type**: `IHostedService` (BackgroundService)

#### Workflow
```
1. Start at 03:00 UTC
   ↓
2. Fetch 90-day feed from ECB
   ↓
3. Parse XML and extract rates
   ↓
4. Upsert rates into database
   ↓
5. Detect gaps (missing dates)
   ↓
6. If gaps found (excluding weekends)
   ↓
7. Fetch historical feed from ECB
   ↓
8. Backfill missing dates
   ↓
9. Log summary and wait 24 hours
```

#### Gap Detection
```csharp
// Get date range
var minDate = await db.ExchangeRates.MinAsync(e => e.Date);
var maxDate = await db.ExchangeRates.MaxAsync(e => e.Date);

// Find missing dates (excluding weekends)
var allDates = Enumerable.Range(0, (maxDate - minDate).Days + 1)
    .Select(d => minDate.AddDays(d))
    .Where(d => d.DayOfWeek != DayOfWeek.Saturday 
             && d.DayOfWeek != DayOfWeek.Sunday);

var existingDates = await db.ExchangeRates
    .Select(e => e.Date)
    .Distinct()
    .ToListAsync();

var missingDates = allDates.Except(existingDates).ToList();
```

#### Retry Logic
```csharp
for (int attempt = 1; attempt <= 3; attempt++)
{
    try
    {
        return await FetchRatesAsync();
    }
    catch (HttpRequestException ex)
    {
        if (attempt == 3) throw;
        
        var delay = TimeSpan.FromSeconds(Math.Pow(3, attempt)); // 3s, 9s, 27s
        await Task.Delay(delay);
    }
}
```

### API Integration

#### Currency Endpoints
```http
GET /api/finance/currencies
GET /api/finance/currencies/exchange-rate?from=USD&to=EUR&date=2025-11-09
GET /api/finance/currencies/convert?amount=100&from=USD&to=EUR&date=2025-11-09
```

#### Account Aggregation
```http
GET /api/finance/accounts?currency=EUR
GET /api/finance/accounts/total?currency=EUR
```

**Response with Conversion**:
```json
{
  "accountId": "...",
  "name": "US Checking",
  "originalCurrency": "USD",
  "originalBalance": 1000.00,
  "convertedCurrency": "EUR",
  "convertedBalance": 920.15
}
```

**Portfolio Total**:
```json
{
  "currency": "EUR",
  "totalBalance": 15432.78,
  "accountCount": 5,
  "calculatedAt": "2025-11-09T12:34:56Z"
}
```

## Error Handling

### Missing Rates
```csharp
var rate = await GetRateAsync(date, from, to);
if (rate == null)
{
    // Option 1: Use latest available rate
    rate = await GetRateAsync(DateOnly.MaxValue, from, to);
    
    // Option 2: Return null and let caller decide
    return null;
    
    // Option 3: Throw exception
    throw new ExchangeRateNotFoundException(...);
}
```

### Network Failures
```csharp
try
{
    await FetchRatesAsync();
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Failed to fetch exchange rates from ECB");
    // Continue with existing data
}
```

### Invalid XML
```csharp
try
{
    var doc = XDocument.Parse(xml);
}
catch (XmlException ex)
{
    _logger.LogError(ex, "Invalid XML received from ECB");
    return Enumerable.Empty<ExchangeRate>();
}
```

## Performance Considerations

### Database Queries
- **Index**: `(Date, TargetCurrency)` for fast lookups
- **No Tracking**: Use `AsNoTracking()` for read-only queries
- **Batch Inserts**: Bulk insert rates in single transaction

### Memory Usage
- **Cache Size**: ~1MB for 90 days × 40 currencies × 8 bytes
- **Cache Eviction**: Automatic after 1 hour
- **No Memory Leaks**: BehaviorSubject properly disposed

### API Response Time
- **With Cache**: < 10ms (in-memory lookup)
- **Without Cache**: < 50ms (database query)
- **Triangular Conversion**: < 20ms (2 cache/DB lookups)

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task GetRateAsync_SameCurrency_ReturnsOne()
{
    var rate = await service.GetRateAsync(date, "EUR", "EUR");
    Assert.Equal(1.0m, rate);
}

[Fact]
public async Task ConvertAmount_UsdToGbp_UsesTriangularArbitrage()
{
    // Arrange: EUR→USD = 1.10, EUR→GBP = 0.85
    // Expected: USD→GBP = 0.85 / 1.10 = 0.7727
    var result = await service.ConvertAmountAsync(100, "USD", "GBP", date);
    Assert.Equal(77.27m, result, 2);
}
```

### Integration Tests
```csharp
[Fact]
public async Task BackgroundJob_FetchesRatesAndStoresInDatabase()
{
    await job.StartAsync(CancellationToken.None);
    await Task.Delay(TimeSpan.FromSeconds(5));
    
    var rates = await db.ExchangeRates.ToListAsync();
    Assert.NotEmpty(rates);
    Assert.Contains(rates, r => r.TargetCurrency == "USD");
}
```

### Manual Testing
1. **Verify Daily Updates**: Check database next day at 03:05 UTC
2. **Test Conversions**: Use Swagger to test currency endpoints
3. **Check Logs**: Review background job execution logs
4. **Gap Detection**: Delete a rate and verify backfill

## Monitoring

### Logging
```csharp
_logger.LogInformation(
    "Exchange rate update completed: {RateCount} rates from {Source}",
    rates.Count,
    source);

_logger.LogWarning(
    "Detected {GapCount} missing dates, backfilling from historical feed",
    gaps.Count);

_logger.LogError(ex,
    "Failed to fetch exchange rates after {Attempts} attempts",
    maxAttempts);
```

### Metrics
- Last update timestamp
- Number of rates in database
- Number of supported currencies
- Cache hit ratio
- Background job success/failure rate

## Future Enhancements

### Phase 1 (Completed)
✅ ECB integration  
✅ Background job with gap detection  
✅ Currency conversion API  
✅ Portfolio aggregation  
✅ Transaction exchange rates

### Phase 2 (Future)
- [ ] Manual exchange rate entry for unsupported currencies
- [ ] Exchange rate history charts
- [ ] Currency conversion alerts (rate thresholds)
- [ ] Multiple data sources (fallback to Fixer.io if ECB down)
- [ ] Cryptocurrency support (BTC, ETH)

### Phase 3 (Future)
- [ ] Predictive exchange rate forecasting
- [ ] Optimal currency conversion suggestions
- [ ] Multi-currency budgets
- [ ] Tax reporting with currency conversion

## Related Documentation
- [ADR-002: ECB Integration](../09_architecture_decisions/ADR-002-ecb-exchange-rates.md)
- [Finance Domain Building Blocks](../05_building_block_view/finance_domain.md)
- [Runtime View: Currency Conversion](../06_runtime_view/currency_conversion.md)

## References
- ECB: https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html
- ISO 4217: https://www.iso.org/iso-4217-currency-codes.html
- Triangular Arbitrage: https://en.wikipedia.org/wiki/Triangular_arbitrage
