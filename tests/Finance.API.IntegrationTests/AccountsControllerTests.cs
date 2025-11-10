using System.Net;
using System.Net.Http.Json;
using Finance.API.DTOs;
using FluentAssertions;

namespace Finance.API.IntegrationTests;

/// <summary>
/// Integration tests for the Accounts API endpoints.
/// Tests account CRUD operations against a real database using Aspire testing.
/// </summary>
public class AccountsControllerTests : IClassFixture<AspireAppFixture>
{
    private readonly HttpClient _httpClient;

    public AccountsControllerTests(AspireAppFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task GetAccounts_ReturnsOk()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/finance/accounts");

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAccount_ValidData_ReturnsCreated()
    {
        // Arrange
        var newAccount = new CreateAccountDto(
            Name: $"Test Account {Guid.NewGuid()}",
            IBAN: "DE89370400440532013000",
            Currency: "EUR",
            InitialBalance: 1000.00m
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/finance/accounts", newAccount);

        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdAccount = await response.Content.ReadFromJsonAsync<AccountDto>();
        createdAccount.Should().NotBeNull();
        createdAccount!.Name.Should().Be(newAccount.Name);
        createdAccount.Currency.Should().Be(newAccount.Currency);
        createdAccount.CurrentBalance.Should().Be(newAccount.InitialBalance);
    }

    [Fact]
    public async Task CreateAccount_InvalidCurrency_ReturnsBadRequest()
    {
        // Arrange
        var invalidAccount = new CreateAccountDto(
            Name: "Invalid Account",
            IBAN: "DE89370400440532013001",
            Currency: "XXX", // Invalid currency code
            InitialBalance: 100
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/finance/accounts", invalidAccount);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAccountById_ExistingAccount_ReturnsOk()
    {
        // Arrange - Create an account first
        var newAccount = new CreateAccountDto(
            Name: $"Account for Get {Guid.NewGuid()}",
            IBAN: "DE89370400440532013002",
            Currency: "USD",
            InitialBalance: 500
        );

        var createResponse = await _httpClient.PostAsJsonAsync("/api/finance/accounts", newAccount);
        createResponse.Should().BeSuccessful();
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        // Act
        var response = await _httpClient.GetAsync($"/api/finance/accounts/{createdAccount!.AccountId}");

        // Assert
        response.Should().BeSuccessful();
        var account = await response.Content.ReadFromJsonAsync<AccountDto>();
        account.Should().NotBeNull();
        account!.AccountId.Should().Be(createdAccount.AccountId);
        account.Name.Should().Be(newAccount.Name);
    }

    [Fact]
    public async Task UpdateAccount_ValidData_ReturnsOk()
    {
        // Arrange - Create an account first
        var createDto = new CreateAccountDto(
            Name: $"Account to Update {Guid.NewGuid()}",
            IBAN: "DE89370400440532013003",
            Currency: "EUR",
            InitialBalance: 1000
        );

        var createResponse = await _httpClient.PostAsJsonAsync("/api/finance/accounts", createDto);
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        var updateDto = new UpdateAccountDto(
            Name: "Updated Account Name",
            IBAN: "DE89370400440532013004"
        );

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/finance/accounts/{createdAccount!.AccountId}", updateDto);

        // Assert
        response.Should().BeSuccessful();
        var updatedAccount = await response.Content.ReadFromJsonAsync<AccountDto>();
        updatedAccount.Should().NotBeNull();
        updatedAccount!.Name.Should().Be(updateDto.Name);
        updatedAccount.Currency.Should().Be(createDto.Currency); // Should remain unchanged
    }

    [Fact]
    public async Task DeleteAccount_ExistingAccount_ReturnsNoContent()
    {
        // Arrange - Create an account first
        var createDto = new CreateAccountDto(
            Name: $"Account to Delete {Guid.NewGuid()}",
            IBAN: "DE89370400440532013005",
            Currency: "GBP",
            InitialBalance: 250
        );

        var createResponse = await _httpClient.PostAsJsonAsync("/api/finance/accounts", createDto);
        var createdAccount = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/finance/accounts/{createdAccount!.AccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify account is deleted
        var getResponse = await _httpClient.GetAsync($"/api/finance/accounts/{createdAccount.AccountId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountById_NonExistentAccount_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.GetAsync($"/api/finance/accounts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
