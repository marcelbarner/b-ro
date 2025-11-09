# Technical Debt

## Overview
This document tracks known technical debt items that should be addressed in future iterations. Technical debt represents deliberate or accidental shortcuts taken during development that may impact maintainability, performance, or functionality.

## Currency Management

### High Priority

#### 1. Missing Frontend Implementation
**Status**: Partially Implemented  
**Added**: 2025-11-09  
**Impact**: High

**Description:**  
Frontend UI for currency features is incomplete. Currency service exists but no UI components for:
- Currency selector in toolbar
- Dual currency display (original + converted)
- Portfolio total widget
- Transaction currency conversion preview

**Consequences:**
- Users cannot leverage multi-currency features
- Backend endpoints unused
- ROI of currency implementation unrealized

**Remediation Plan:**
1. Create `CurrencySelectorComponent` for toolbar
2. Update `AccountListComponent` to show converted balances
3. Add `PortfolioTotalComponent` widget to dashboard
4. Enhance transaction forms with conversion preview
5. Add i18n translations for currency UI

**Estimated Effort**: 16-24 hours  
**Target**: Sprint 12

---

#### 2. No Unit Tests for Currency Services
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: High

**Description:**  
`CurrencyService`, `AccountAggregationService`, and `ExchangeRateUpdateJob` lack comprehensive unit tests.

**Consequences:**
- Difficult to refactor safely
- Risk of regression bugs
- No validation of triangular arbitrage logic
- Background job failures hard to diagnose

**Remediation Plan:**
1. Create `Finance.Infrastructure.Tests` project (exists but empty)
2. Test `CurrencyService`:
   - Same currency returns original amount
   - Direct EUR conversion (forward and inverse)
   - Triangular arbitrage USD→GBP
   - Caching behavior
   - Missing rate handling
3. Test `AccountAggregationService`:
   - Total calculation across currencies
   - Single account conversion
   - Edge cases (zero accounts, null rates)
4. Test `ExchangeRateUpdateJob`:
   - Mock HTTP responses
   - Gap detection logic
   - Retry behavior
   - Database upsert

**Estimated Effort**: 12-16 hours  
**Target**: Sprint 11

---

### Medium Priority

#### 3. No Monitoring/Alerting for Background Job
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: Medium

**Description:**  
`ExchangeRateUpdateJob` runs daily but has no monitoring, alerting, or health checks.

**Consequences:**
- Silent failures (job stops, no one notices)
- Stale exchange rate data
- Users see incorrect conversions
- No visibility into job execution

**Remediation Plan:**
1. Add health check endpoint: `GET /health/exchange-rates`
   - Check last update timestamp
   - Warn if > 2 days old
   - Return 503 if > 7 days old
2. Log to Application Insights/Seq
3. Add Prometheus metrics:
   - `exchange_rate_update_duration_seconds`
   - `exchange_rate_update_success_total`
   - `exchange_rate_update_failure_total`
4. Configure alerts:
   - No update in 48 hours
   - 3 consecutive failures

**Estimated Effort**: 4-6 hours  
**Target**: Sprint 12

---

#### 4. Hardcoded ECB URLs
**Status**: Technical Debt  
**Added**: 2025-11-09  
**Impact**: Medium

**Description:**  
ECB feed URLs are hardcoded in `EcbExchangeRateProvider`:
```csharp
private const string ECB_90_DAY_URL = "https://www.ecb.europa.eu/...";
private const string ECB_HISTORICAL_URL = "https://www.ecb.europa.eu/...";
```

**Consequences:**
- Cannot switch providers without code change
- Cannot mock for testing
- Hard to add fallback sources

**Remediation Plan:**
1. Move URLs to `appsettings.json`:
   ```json
   "ExchangeRates": {
     "Ecb": {
       "Daily90Url": "...",
       "HistoricalUrl": "..."
     }
   }
   ```
2. Create configuration class:
   ```csharp
   public class ExchangeRateOptions
   {
       public EcbOptions Ecb { get; set; }
   }
   ```
3. Inject `IOptions<ExchangeRateOptions>` into provider

**Estimated Effort**: 1-2 hours  
**Target**: Sprint 11

---

#### 5. No Support for Unsupported Currencies
**Status**: Limitation  
**Added**: 2025-11-09  
**Impact**: Medium

**Description:**  
ECB only provides ~40 currencies. Users cannot track accounts in exotic currencies (e.g., BTN, AOA, MMK).

**Consequences:**
- Users with exotic currency accounts cannot use the app
- Workaround: Create EUR account and manually convert

**Remediation Plan:**
1. Add manual exchange rate entry feature:
   - UI for entering custom rates
   - Store in `ExchangeRates` table with `Source = 'Manual'`
   - Allow editing/deletion of manual rates
2. Fallback to manual rates if ECB unavailable
3. Mark manual rates in UI (icon/badge)

**Estimated Effort**: 8-12 hours  
**Target**: Sprint 13

---

### Low Priority

#### 6. Transaction Exchange Rate Not Auto-Populated
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: Low

**Description:**  
When creating a transaction in non-EUR currency, `ExchangeRateToEUR` is not automatically populated. It stays NULL.

**Consequences:**
- `ConvertedAmountEUR` always NULL for new transactions
- Historical reporting inaccurate
- Users must manually track rates

**Remediation Plan:**
1. In `TransactionsController.CreateTransaction()`:
   ```csharp
   if (dto.Currency != "EUR")
   {
       var rate = await _currencyService.GetRateAsync(
           DateOnly.FromDateTime(dto.Date), 
           "EUR", 
           dto.Currency);
       
       transaction = new Transaction(
           ...,
           exchangeRateToEUR: rate.HasValue ? 1 / rate.Value : null
       );
   }
   ```
2. Update `CreateTransactionDto` to accept optional `ExchangeRateToEUR`
3. Allow manual override in UI (for bank-specific rates)

**Estimated Effort**: 2-4 hours  
**Target**: Sprint 12

---

#### 7. Cache Eviction Not Efficient
**Status**: Suboptimal  
**Added**: 2025-11-09  
**Impact**: Low

**Description:**  
Cache uses sliding expiration (1 hour) but doesn't invalidate on database updates. If background job updates rates, cache still serves stale data for up to 1 hour.

**Consequences:**
- Users see outdated rates for up to 1 hour after update
- Minor - only affects daily 03:00 UTC update window

**Remediation Plan:**
1. Add cache invalidation in `ExchangeRateUpdateJob`:
   ```csharp
   await UpdateRatesAsync();
   _cache.Remove("latest_rates");
   _cache.Remove("supported_currencies");
   // Clear all rate caches (use cache tag)
   ```
2. Or: Use distributed cache (Redis) with change feed
3. Or: Reduce TTL to 15 minutes

**Estimated Effort**: 2-3 hours  
**Target**: Sprint 13

---

#### 8. No Cryptocurrency Support
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: Low

**Description:**  
Cannot track cryptocurrency wallets (BTC, ETH, USDT, etc.) or crypto exchange accounts.

**Consequences:**
- Users with crypto assets must use separate app
- No unified portfolio view

**Remediation Plan:**
1. Integrate with CoinGecko API (free tier: 10-50 calls/min)
2. Add cryptocurrency codes to supported currencies
3. Background job for crypto prices (separate from ECB)
4. Handle high volatility (multiple prices per day)
5. Display warnings about volatility

**Estimated Effort**: 16-24 hours  
**Target**: Backlog (low priority)

---

## General Technical Debt

### 9. Missing Integration Tests
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: Medium

**Description:**  
No integration tests for API endpoints. Only manual testing via Swagger.

**Remediation Plan:**
1. Create `Finance.API.Tests` project
2. Use `WebApplicationFactory<Program>`
3. Test happy paths for all endpoints
4. Test error cases (404, 400, 409)
5. Test currency conversion endpoints end-to-end

**Estimated Effort**: 8-12 hours  
**Target**: Sprint 11

---

### 10. No Performance Testing
**Status**: Not Implemented  
**Added**: 2025-11-09  
**Impact**: Low

**Description:**  
No load testing or performance benchmarks. Unknown behavior under high load.

**Remediation Plan:**
1. Create k6/JMeter load test scripts
2. Test scenarios:
   - 100 concurrent users fetching accounts
   - Portfolio aggregation with 1000 accounts
   - Background job with 10,000 rates
3. Identify bottlenecks
4. Add database indexes if needed

**Estimated Effort**: 6-8 hours  
**Target**: Sprint 14

---

## Tracking

| ID | Item | Priority | Effort | Target | Status |
|----|------|----------|--------|--------|--------|
| 1  | Frontend Currency UI | High | 16-24h | Sprint 12 | Not Started |
| 2  | Unit Tests | High | 12-16h | Sprint 11 | Not Started |
| 3  | Monitoring/Alerting | Medium | 4-6h | Sprint 12 | Not Started |
| 4  | Configuration for URLs | Medium | 1-2h | Sprint 11 | Not Started |
| 5  | Manual Rate Entry | Medium | 8-12h | Sprint 13 | Not Started |
| 6  | Auto-populate ExchangeRate | Low | 2-4h | Sprint 12 | Not Started |
| 7  | Cache Invalidation | Low | 2-3h | Sprint 13 | Not Started |
| 8  | Cryptocurrency Support | Low | 16-24h | Backlog | Not Started |
| 9  | Integration Tests | Medium | 8-12h | Sprint 11 | Not Started |
| 10 | Performance Testing | Low | 6-8h | Sprint 14 | Not Started |

**Total Estimated Effort**: 75-107 hours

---

*Last updated: 2025-11-09*
