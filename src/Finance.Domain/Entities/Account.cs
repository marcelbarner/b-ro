namespace Finance.Domain.Entities;

/// <summary>
/// Represents a bank account with transaction history.
/// </summary>
public class Account
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Display name of the account.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// International Bank Account Number.
    /// </summary>
    public string IBAN { get; private set; } = string.Empty;

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Initial balance when account was created.
    /// </summary>
    public decimal InitialBalance { get; private set; }

    /// <summary>
    /// Current balance (calculated from transactions).
    /// </summary>
    public decimal CurrentBalance { get; private set; }

    /// <summary>
    /// When the account was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// When the account was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property for transactions.
    /// </summary>
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    // Private constructor for EF Core
    private Account()
    {
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    public Account(string name, string iban, string currency, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN cannot be empty.", nameof(iban));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        AccountId = Guid.NewGuid();
        Name = name;
        IBAN = iban;
        Currency = currency.ToUpperInvariant();
        InitialBalance = initialBalance;
        CurrentBalance = initialBalance;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the account name and IBAN.
    /// </summary>
    public void Update(string name, string iban)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN cannot be empty.", nameof(iban));

        Name = name;
        IBAN = iban;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Recalculates the current balance from initial balance and all transactions.
    /// </summary>
    public void UpdateBalance()
    {
        CurrentBalance = InitialBalance + Transactions.Sum(t => t.Amount);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Validates if the account has sufficient balance for a transaction.
    /// </summary>
    public bool HasSufficientBalance(decimal amount)
    {
        return CurrentBalance + amount >= 0;
    }
}
