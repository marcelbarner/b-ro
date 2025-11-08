using Finance.Domain.Entities;

namespace Finance.Domain.Repositories;

/// <summary>
/// Repository interface for account persistence.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets an account by its unique identifier.
    /// </summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts.
    /// </summary>
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets accounts with pagination.
    /// </summary>
    Task<IReadOnlyList<Account>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new account.
    /// </summary>
    Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an account.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an account with the given IBAN already exists.
    /// </summary>
    Task<bool> ExistsWithIbanAsync(string iban, CancellationToken cancellationToken = default);
}
