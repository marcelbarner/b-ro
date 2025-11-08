using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Account entity.
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly FinanceDbContext _context;

    public AccountRepository(FinanceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .OrderBy(a => a.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync(new object[] { id }, cancellationToken);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsWithIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.IBAN == iban, cancellationToken);
    }
}
