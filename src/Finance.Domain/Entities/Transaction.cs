namespace Finance.Domain.Entities;

/// <summary>
/// Represents a financial transaction on an account.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier for the transaction.
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// The account this transaction belongs to.
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public Account Account { get; private set; } = null!;

    /// <summary>
    /// Transaction amount (positive for credits, negative for debits).
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Type of transaction.
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Description or memo for the transaction.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Date and time of the transaction.
    /// </summary>
    public DateTimeOffset Date { get; private set; }

    /// <summary>
    /// Link to counter-transaction (for transfers).
    /// </summary>
    public Guid? CounterTransactionId { get; private set; }

    /// <summary>
    /// Navigation property to counter-transaction.
    /// </summary>
    public Transaction? CounterTransaction { get; private set; }

    /// <summary>
    /// When the transaction was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// When the transaction was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Exchange rate to EUR at the time of transaction creation.
    /// Null if transaction currency is EUR or rate was not available.
    /// </summary>
    public decimal? ExchangeRateToEUR { get; private set; }

    /// <summary>
    /// Gets the converted amount in EUR.
    /// For EUR transactions, returns the original amount.
    /// For other currencies, converts using the stored exchange rate.
    /// </summary>
    public decimal ConvertedAmountEUR => 
        Currency == "EUR" 
            ? Amount 
            : Amount * (ExchangeRateToEUR ?? 1.0m);

    // Private constructor for EF Core
    private Transaction()
    {
    }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    public Transaction(
        Guid accountId,
        decimal amount,
        string currency,
        TransactionType type,
        string description,
        DateTimeOffset date,
        decimal? exchangeRateToEUR = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        TransactionId = Guid.NewGuid();
        AccountId = accountId;
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        Type = type;
        Description = description;
        Date = date;
        ExchangeRateToEUR = exchangeRateToEUR;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Links this transaction to a counter-transaction (for transfers).
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Amounts must match (absolute value)
    /// - Amounts must have opposite signs
    /// - Currencies must match
    /// - A transaction can only have one counter-transaction
    /// - Cannot link to itself
    /// </remarks>
    public void LinkCounterTransaction(Transaction counterTransaction)
    {
        if (counterTransaction == null)
            throw new ArgumentNullException(nameof(counterTransaction));

        if (counterTransaction.TransactionId == TransactionId)
            throw new InvalidOperationException("Cannot link a transaction to itself.");

        if (CounterTransactionId.HasValue)
            throw new InvalidOperationException("This transaction is already linked to a counter-transaction.");

        if (counterTransaction.CounterTransactionId.HasValue)
            throw new InvalidOperationException("The counter-transaction is already linked to another transaction.");

        if (Math.Abs(Amount) != Math.Abs(counterTransaction.Amount))
            throw new InvalidOperationException("Counter-transaction amounts must match (absolute value).");

        if (Math.Sign(Amount) == Math.Sign(counterTransaction.Amount))
            throw new InvalidOperationException("Counter-transaction amounts must have opposite signs.");

        if (Currency != counterTransaction.Currency)
            throw new InvalidOperationException($"Currency mismatch: {Currency} != {counterTransaction.Currency}");

        CounterTransactionId = counterTransaction.TransactionId;
        counterTransaction.CounterTransactionId = TransactionId;
        
        UpdatedAt = DateTimeOffset.UtcNow;
        counterTransaction.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Unlinks this transaction from its counter-transaction.
    /// </summary>
    public void UnlinkCounterTransaction()
    {
        if (!CounterTransactionId.HasValue)
            return;

        if (CounterTransaction != null)
        {
            CounterTransaction.CounterTransactionId = null;
            CounterTransaction.UpdatedAt = DateTimeOffset.UtcNow;
        }

        CounterTransactionId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Types of financial transactions.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Money added to account.
    /// </summary>
    Deposit,

    /// <summary>
    /// Money removed from account.
    /// </summary>
    Withdrawal,

    /// <summary>
    /// Transfer to/from another account.
    /// </summary>
    Transfer,

    /// <summary>
    /// Account fee or charge.
    /// </summary>
    Fee,

    /// <summary>
    /// Interest earned or charged.
    /// </summary>
    Interest
}
