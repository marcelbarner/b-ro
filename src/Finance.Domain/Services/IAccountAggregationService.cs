namespace Finance.Domain.Services;

/// <summary>
/// Service for aggregating account balances across multiple currencies.
/// </summary>
public interface IAccountAggregationService
{
    /// <summary>
    /// Gets the total balance across all accounts in the specified currency.
    /// </summary>
    /// <param name="targetCurrency">The target currency for the total (ISO 4217).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total balance in the target currency.</returns>
    Task<decimal> GetTotalBalanceInCurrencyAsync(
        string targetCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account's balance converted to the specified currency.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="targetCurrency">The target currency (ISO 4217).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Account balance in the target currency, or null if account not found.</returns>
    Task<decimal?> GetAccountBalanceInCurrencyAsync(
        Guid accountId,
        string targetCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts with their balances converted to the specified currency.
    /// </summary>
    /// <param name="targetCurrency">The target currency (ISO 4217).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of account IDs to converted balances.</returns>
    Task<Dictionary<Guid, decimal>> GetAllAccountsWithConvertedBalancesAsync(
        string targetCurrency,
        CancellationToken cancellationToken = default);
}
