using Finance.Domain.Services;
using Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure.Services;

/// <summary>
/// Service for aggregating account balances across multiple currencies.
/// </summary>
public class AccountAggregationService : IAccountAggregationService
{
    private readonly FinanceDbContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<AccountAggregationService> _logger;

    public AccountAggregationService(
        FinanceDbContext context,
        ICurrencyService currencyService,
        ILogger<AccountAggregationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<decimal> GetTotalBalanceInCurrencyAsync(
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            throw new ArgumentException("Target currency cannot be null or empty.", nameof(targetCurrency));
        }

        targetCurrency = targetCurrency.ToUpperInvariant();

        _logger.LogInformation("Calculating total balance in {Currency}", targetCurrency);

        var accounts = await _context.Accounts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (accounts.Count == 0)
        {
            _logger.LogInformation("No accounts found, returning zero balance");
            return 0m;
        }

        decimal total = 0m;

        foreach (var account in accounts)
        {
            var convertedBalance = await ConvertBalanceAsync(
                account.CurrentBalance,
                account.Currency,
                targetCurrency,
                cancellationToken);

            total += convertedBalance;

            _logger.LogDebug(
                "Account {AccountId} ({AccountCurrency}): {OriginalBalance} → {ConvertedBalance} {TargetCurrency}",
                account.AccountId,
                account.Currency,
                account.CurrentBalance,
                convertedBalance,
                targetCurrency);
        }

        _logger.LogInformation(
            "Total balance across {AccountCount} accounts: {Total} {Currency}",
            accounts.Count,
            total,
            targetCurrency);

        return total;
    }

    public async Task<decimal?> GetAccountBalanceInCurrencyAsync(
        Guid accountId,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            throw new ArgumentException("Target currency cannot be null or empty.", nameof(targetCurrency));
        }

        targetCurrency = targetCurrency.ToUpperInvariant();

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);

        if (account is null)
        {
            _logger.LogWarning("Account {AccountId} not found", accountId);
            return null;
        }

        var convertedBalance = await ConvertBalanceAsync(
            account.CurrentBalance,
            account.Currency,
            targetCurrency,
            cancellationToken);

        _logger.LogInformation(
            "Account {AccountId} balance: {OriginalBalance} {OriginalCurrency} → {ConvertedBalance} {TargetCurrency}",
            accountId,
            account.CurrentBalance,
            account.Currency,
            convertedBalance,
            targetCurrency);

        return convertedBalance;
    }

    public async Task<Dictionary<Guid, decimal>> GetAllAccountsWithConvertedBalancesAsync(
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            throw new ArgumentException("Target currency cannot be null or empty.", nameof(targetCurrency));
        }

        targetCurrency = targetCurrency.ToUpperInvariant();

        _logger.LogInformation("Getting all accounts with balances in {Currency}", targetCurrency);

        var accounts = await _context.Accounts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, decimal>();

        foreach (var account in accounts)
        {
            var convertedBalance = await ConvertBalanceAsync(
                account.CurrentBalance,
                account.Currency,
                targetCurrency,
                cancellationToken);

            result[account.AccountId] = convertedBalance;
        }

        _logger.LogInformation(
            "Retrieved {AccountCount} accounts with balances in {Currency}",
            result.Count,
            targetCurrency);

        return result;
    }

    private async Task<decimal> ConvertBalanceAsync(
        decimal balance,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken)
    {
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        // No conversion needed if currencies match
        if (fromCurrency == toCurrency)
        {
            return balance;
        }

        // Use currency service to convert (uses latest rate)
        var convertedAmount = await _currencyService.ConvertAmountAsync(
            balance,
            fromCurrency,
            toCurrency,
            null, // Use latest rate
            cancellationToken);

        return convertedAmount ?? throw new InvalidOperationException(
            $"Cannot convert {fromCurrency} to {toCurrency}: exchange rate not available.");
    }
}
