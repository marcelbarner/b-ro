# Finance API Integration Tests

This project contains integration tests for the Finance API using Aspire distributed application testing.

## Overview

These tests verify the Finance API endpoints against a real PostgreSQL database running in a containerized environment orchestrated by .NET Aspire. The tests cover:

- **Account Management**: CRUD operations, validation, and balance tracking
- **Transaction Management**: Deposits, withdrawals, transfers, counter-transaction linking
- **Currency Conversion**: Exchange rate retrieval and multi-currency operations

## Technology Stack

- **xUnit**: Test framework
- **Aspire.Hosting.Testing**: Distributed application testing with automatic container orchestration
- **FluentAssertions**: Readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing**: HTTP client for API testing

## Running the Tests

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop (for PostgreSQL container)
- All dependencies restored via central package management

### Run All Tests

```pwsh
dotnet test tests/Finance.API.IntegrationTests/Finance.API.IntegrationTests.csproj
```

### Run Specific Test Class

```pwsh
dotnet test --filter "FullyQualifiedName~AccountsControllerTests"
dotnet test --filter "FullyQualifiedName~TransactionsControllerTests"
dotnet test --filter "FullyQualifiedName~CurrenciesControllerTests"
```

### Run with Detailed Output

```pwsh
dotnet test --logger "console;verbosity=detailed"
```

## Test Structure

### AspireAppFixture

Base fixture that:
1. Builds the Aspire distributed application from `BRo.AppHost`
2. Starts all services (PostgreSQL, Finance API, frontend)
3. Provides an HTTP client configured for the Finance API
4. Handles cleanup after tests complete

Used via `IClassFixture<AspireAppFixture>` to share the application instance across test methods in a class.

### Test Classes

#### AccountsControllerTests
- `GetAccounts_ReturnsOk` - Verify account listing
- `CreateAccount_ValidData_ReturnsCreated` - Test account creation with valid data
- `CreateAccount_InvalidCurrency_ReturnsBadRequest` - Validation error handling
- `GetAccountById_ExistingAccount_ReturnsOk` - Retrieve specific account
- `UpdateAccount_ValidData_ReturnsOk` - Update account details
- `DeleteAccount_ExistingAccount_ReturnsNoContent` - Account deletion
- `GetAccountById_NonExistentAccount_ReturnsNotFound` - Error handling

#### TransactionsControllerTests
- `CreateTransaction_ValidWithdrawal_ReturnsCreated` - Withdrawal transaction
- `CreateTransaction_ValidDeposit_ReturnsCreated` - Deposit transaction
- `CreateTransaction_InsufficientBalance_ReturnsBadRequest` - Balance validation
- `CreateTransaction_UpdatesAccountBalance` - Verify balance updates
- `CreateTransfer_ValidTransfer_ReturnsCreated` - Transfer between accounts
- `CreateTransfer_InsufficientBalance_ReturnsBadRequest` - Transfer validation
- `CreateTransfer_DifferentCurrencies_ReturnsCreated` - Cross-currency transfer
- `DeleteTransaction_ExistingTransaction_ReturnsNoContent` - Transaction deletion
- `LinkCounterTransaction_ValidLink_ReturnsOk` - Link related transactions
- `GetTransactionById_ExistingTransaction_ReturnsOk` - Retrieve transaction

#### CurrenciesControllerTests
- `ConvertCurrency_ValidConversion_ReturnsOk` - Currency conversion
- `ConvertCurrency_SameCurrency_ReturnsOriginalAmount` - Same currency handling
- `ConvertCurrency_InvalidCurrency_ReturnsBadRequest` - Invalid currency validation
- `ConvertCurrency_NegativeAmount_ReturnsBadRequest` - Amount validation
- `ConvertCurrency_VariousCurrencyPairs_ReturnsValidConversion` - Multiple currency pairs
- `GetExchangeRate_ValidPair_ReturnsRate` - Exchange rate retrieval
- `GetExchangeRate_WithSpecificDate_ReturnsHistoricalRate` - Historical rates

## Test Data Management

Tests use:
- Unique identifiers (GUIDs) for test data to avoid conflicts
- Random IBANs for account creation
- Realistic currency codes (EUR, USD, GBP, JPY, CHF)
- Cleanup handled automatically by Aspire test infrastructure

Each test class instance gets a fresh Aspire application, ensuring test isolation.

## Best Practices

1. **Isolation**: Each test creates its own test data with unique identifiers
2. **Realistic Data**: Uses valid IBANs, currency codes, and realistic amounts
3. **Clear Assertions**: FluentAssertions for readable test validation
4. **AAA Pattern**: Arrange, Act, Assert structure in all tests
5. **Helper Methods**: `CreateTestAccountAsync` reduces code duplication

## Troubleshooting

### Tests Fail to Start

**Issue**: "Could not start Aspire application"
**Solution**: Ensure Docker Desktop is running and PostgreSQL container can start

### Connection Errors

**Issue**: "Unable to connect to Finance API"
**Solution**: Check that the `financeapi` service name in `BRo.AppHost` matches the HTTP client creation

### Slow Test Execution

**Expected**: First test in each class is slower (application startup)
**Improvement**: Tests in the same class share the Aspire instance via `IClassFixture`

## CI/CD Integration

These tests are designed to run in CI/CD pipelines:

```yaml
- name: Run Integration Tests
  run: dotnet test tests/Finance.API.IntegrationTests/Finance.API.IntegrationTests.csproj --logger trx
  env:
    DOCKER_HOST: tcp://localhost:2375  # Adjust for CI environment
```

## Related Documentation

- [Issue #10: Finance E2E Tests](../../docs/09_architecture_decisions/testing_strategy.md)
- [Aspire Testing Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/testing/overview)
- [Finance API Documentation](../../src/Finance.API/README.md)
