using Finance.Domain.Entities;

namespace Finance.Domain.Repositories;

/// <summary>
/// Repository interface for transaction persistence.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Gets a transaction by its unique identifier.
    /// </summary>
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions for a specific account.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions for an account within a date range.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetByAccountIdAndDateRangeAsync(
        Guid accountId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a transaction with its counter-transaction loaded.
    /// </summary>
    Task<Transaction?> GetWithCounterTransactionAsync(Guid id, CancellationToken cancellationToken = default);
}
