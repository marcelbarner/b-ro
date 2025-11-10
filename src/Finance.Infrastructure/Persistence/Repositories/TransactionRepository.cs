using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Transaction entity.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    public TransactionRepository(FinanceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAndDateRangeAsync(
        Guid accountId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions.FindAsync(new object[] { id }, cancellationToken);
        if (transaction != null)
        {
            // Unlink counter-transaction first
            if (transaction.CounterTransactionId.HasValue)
            {
                var counterTransaction = await _context.Transactions
                    .FindAsync(new object[] { transaction.CounterTransactionId.Value }, cancellationToken);

                if (counterTransaction != null)
                {
                    counterTransaction.UnlinkCounterTransaction();
                }
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Transaction?> GetWithCounterTransactionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.CounterTransaction)
            .FirstOrDefaultAsync(t => t.TransactionId == id, cancellationToken);
    }
}
