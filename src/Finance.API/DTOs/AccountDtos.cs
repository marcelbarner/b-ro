namespace Finance.API.DTOs;

/// <summary>
/// DTO for account information.
/// </summary>
public record AccountDto(
    Guid AccountId,
    string Name,
    string IBAN,
    string Currency,
    decimal InitialBalance,
    decimal CurrentBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// DTO for account information with converted balance.
/// </summary>
public record AccountWithConvertedBalanceDto(
    Guid AccountId,
    string Name,
    string IBAN,
    string OriginalCurrency,
    decimal OriginalBalance,
    string ConvertedCurrency,
    decimal ConvertedBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// DTO for total portfolio balance.
/// </summary>
public record PortfolioTotalDto(
    string Currency,
    decimal TotalBalance,
    int AccountCount,
    DateTimeOffset CalculatedAt
);

/// <summary>
/// DTO for creating a new account.
/// </summary>
public record CreateAccountDto(
    string Name,
    string IBAN,
    string Currency,
    decimal InitialBalance
);

/// <summary>
/// DTO for updating an account.
/// </summary>
public record UpdateAccountDto(
    string Name,
    string IBAN
);
