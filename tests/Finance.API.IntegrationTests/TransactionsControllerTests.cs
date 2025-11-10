using System.Net;
using System.Net.Http.Json;
using Finance.API.DTOs;
using Finance.Domain.Entities;
using FluentAssertions;

namespace Finance.API.IntegrationTests;

/// <summary>
/// Integration tests for the Transactions API endpoints.
/// Tests transaction CRUD operations, transfers, and counter-transaction linking.
/// </summary>
public class TransactionsControllerTests : IClassFixture<AspireAppFixture>
{
    private readonly HttpClient _httpClient;

    public TransactionsControllerTests(AspireAppFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    private async Task<AccountDto> CreateTestAccountAsync(string name, string currency = "EUR", decimal initialBalance = 1000)
    {
        var account = new CreateAccountDto(
            Name: name,
            IBAN: $"DE{Random.Shared.Next(10, 99)}{Random.Shared.Next(100000000, 999999999):D10}",
            Currency: currency,
            InitialBalance: initialBalance
        );

        var response = await _httpClient.PostAsJsonAsync("/api/finance/accounts", account);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }

    [Fact]
    public async Task GetTransactions_ForAccount_ReturnsOk()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Account for Transactions {Guid.NewGuid()}");

        // Act
        var response = await _httpClient.GetAsync($"/api/finance/accounts/{account.AccountId}/transactions");

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTransaction_ValidWithdrawal_ReturnsCreated()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Withdrawal Account {Guid.NewGuid()}", "EUR", 1000);
        var transaction = new CreateTransactionDto(
            Amount: -50.00m,
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "Test Expense",
            Date: DateTimeOffset.UtcNow
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTransaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
        createdTransaction.Should().NotBeNull();
        createdTransaction!.Amount.Should().Be(transaction.Amount);
        createdTransaction.Type.Should().Be(TransactionType.Withdrawal);
        createdTransaction.Description.Should().Be(transaction.Description);
    }

    [Fact]
    public async Task CreateTransaction_ValidDeposit_ReturnsCreated()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Deposit Account {Guid.NewGuid()}", "USD", 500);
        var transaction = new CreateTransactionDto(
            Amount: 100.00m,
            Currency: "USD",
            Type: TransactionType.Deposit,
            Description: "Test Income",
            Date: DateTimeOffset.UtcNow
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTransaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
        createdTransaction.Should().NotBeNull();
        createdTransaction!.Amount.Should().Be(transaction.Amount);
        createdTransaction.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task CreateTransaction_InsufficientBalance_ReturnsBadRequest()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Low Balance {Guid.NewGuid()}", "EUR", 100);
        var transaction = new CreateTransactionDto(
            Amount: -500.00m, // More than available balance
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "Exceeds Balance",
            Date: DateTimeOffset.UtcNow
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_UpdatesAccountBalance()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Balance Test {Guid.NewGuid()}", "EUR", 1000);
        var withdrawalAmount = -150.00m;
        var transaction = new CreateTransactionDto(
            Amount: withdrawalAmount,
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "Test Withdrawal",
            Date: DateTimeOffset.UtcNow
        );

        // Act
        await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);

        // Assert - Check updated balance
        var accountResponse = await _httpClient.GetAsync($"/api/finance/accounts/{account.AccountId}");
        var updatedAccount = await accountResponse.Content.ReadFromJsonAsync<AccountDto>();
        updatedAccount.Should().NotBeNull();
        updatedAccount!.CurrentBalance.Should().Be(1000 + withdrawalAmount); // 1000 - 150 = 850
    }

    [Fact]
    public async Task CreateTransfer_ValidTransfer_ReturnsCreated()
    {
        // Arrange
        var fromAccount = await CreateTestAccountAsync($"From Account {Guid.NewGuid()}", "EUR", 1000);
        var toAccount = await CreateTestAccountAsync($"To Account {Guid.NewGuid()}", "EUR", 500);

        var transfer = new CreateTransferDto(
            FromAccountId: fromAccount.AccountId,
            ToAccountId: toAccount.AccountId,
            Amount: 200.00m,
            Description: "Test Transfer"
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/finance/transfers", transfer);

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<TransferResultDto>();
        result.Should().NotBeNull();
        result!.FromAccountBalance.Should().Be(800); // 1000 - 200
        result.ToAccountBalance.Should().Be(700);    // 500 + 200
    }

    [Fact]
    public async Task CreateTransfer_InsufficientBalance_ReturnsBadRequest()
    {
        // Arrange
        var fromAccount = await CreateTestAccountAsync($"Poor Account {Guid.NewGuid()}", "EUR", 50);
        var toAccount = await CreateTestAccountAsync($"Rich Account {Guid.NewGuid()}", "EUR", 1000);

        var transfer = new CreateTransferDto(
            FromAccountId: fromAccount.AccountId,
            ToAccountId: toAccount.AccountId,
            Amount: 100.00m, // More than available
            Description: "Invalid Transfer"
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/finance/transfers", transfer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransfer_DifferentCurrencies_ReturnsCreated()
    {
        // Arrange
        var eurAccount = await CreateTestAccountAsync($"EUR Account {Guid.NewGuid()}", "EUR", 1000);
        var usdAccount = await CreateTestAccountAsync($"USD Account {Guid.NewGuid()}", "USD", 500);

        var transfer = new CreateTransferDto(
            FromAccountId: eurAccount.AccountId,
            ToAccountId: usdAccount.AccountId,
            Amount: 100.00m,
            Description: "Cross-Currency Transfer"
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/finance/transfers", transfer);

        // Assert - Should work with currency conversion
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DeleteTransaction_ExistingTransaction_ReturnsNoContent()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Delete Test {Guid.NewGuid()}", "EUR", 1000);
        var transaction = new CreateTransactionDto(
            Amount: -50.00m,
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "To Delete",
            Date: DateTimeOffset.UtcNow
        );

        var createResponse = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);
        var createdTransaction = await createResponse.Content.ReadFromJsonAsync<TransactionDto>();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/finance/transactions/{createdTransaction!.TransactionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task LinkCounterTransaction_ValidLink_ReturnsOk()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Link Test {Guid.NewGuid()}", "EUR", 1000);

        // Create two transactions
        var transaction1 = new CreateTransactionDto(
            Amount: -100.00m,
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "Transaction 1",
            Date: DateTimeOffset.UtcNow
        );

        var transaction2 = new CreateTransactionDto(
            Amount: 100.00m,
            Currency: "EUR",
            Type: TransactionType.Deposit,
            Description: "Transaction 2",
            Date: DateTimeOffset.UtcNow
        );

        var response1 = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction1);
        var created1 = await response1.Content.ReadFromJsonAsync<TransactionDto>();

        var response2 = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction2);
        var created2 = await response2.Content.ReadFromJsonAsync<TransactionDto>();

        var linkDto = new LinkCounterTransactionDto(CounterTransactionId: created2!.TransactionId);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/finance/transactions/{created1!.TransactionId}/link-counter",
            linkDto
        );

        // Assert
        response.Should().BeSuccessful();
    }

    [Fact]
    public async Task GetTransactionById_ExistingTransaction_ReturnsOk()
    {
        // Arrange
        var account = await CreateTestAccountAsync($"Get By ID {Guid.NewGuid()}", "EUR", 1000);
        var transaction = new CreateTransactionDto(
            Amount: -25.00m,
            Currency: "EUR",
            Type: TransactionType.Withdrawal,
            Description: "Get Test",
            Date: DateTimeOffset.UtcNow
        );

        var createResponse = await _httpClient.PostAsJsonAsync($"/api/finance/accounts/{account.AccountId}/transactions", transaction);
        var created = await createResponse.Content.ReadFromJsonAsync<TransactionDto>();

        // Act
        var response = await _httpClient.GetAsync($"/api/finance/transactions/{created!.TransactionId}");

        // Assert
        response.Should().BeSuccessful();
        var retrieved = await response.Content.ReadFromJsonAsync<TransactionDto>();
        retrieved.Should().NotBeNull();
        retrieved!.TransactionId.Should().Be(created.TransactionId);
        retrieved.Description.Should().Be(transaction.Description);
    }
}
