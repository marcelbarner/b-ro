using Finance.Domain.Entities;

namespace Finance.API.DTOs;

/// <summary>
/// DTO for transaction information.
/// </summary>
public record TransactionDto(
    Guid TransactionId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    TransactionType Type,
    string Description,
    DateTimeOffset Date,
    Guid? CounterTransactionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// DTO for creating a new transaction.
/// </summary>
public record CreateTransactionDto(
    decimal Amount,
    string Currency,
    TransactionType Type,
    string Description,
    DateTimeOffset? Date
);

/// <summary>
/// DTO for creating a transfer between accounts.
/// </summary>
public record CreateTransferDto(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Description
);

/// <summary>
/// DTO for transfer result.
/// </summary>
public record TransferResultDto(
    Guid DebitTransactionId,
    Guid CreditTransactionId,
    decimal FromAccountBalance,
    decimal ToAccountBalance
);

/// <summary>
/// DTO for linking counter-transaction.
/// </summary>
public record LinkCounterTransactionDto(
    Guid CounterTransactionId
);
