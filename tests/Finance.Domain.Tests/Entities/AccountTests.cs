using Finance.Domain.Entities;
using FluentAssertions;

namespace Finance.Domain.Tests.Entities;

public class AccountTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAccount()
    {
        // Arrange & Act
        var account = new Account("Checking Account", "DE89370400440532013000", "EUR", 1000m);

        // Assert
        account.Name.Should().Be("Checking Account");
        account.IBAN.Should().Be("DE89370400440532013000");
        account.Currency.Should().Be("EUR");
        account.InitialBalance.Should().Be(1000m);
        account.CurrentBalance.Should().Be(1000m);
        account.AccountId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Account("", "DE89370400440532013000", "EUR", 1000m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Account name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithInvalidCurrencyLength_ShouldThrowException()
    {
        // Act & Assert
        var act = () => new Account("Checking Account", "DE89370400440532013000", "EURO", 1000m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-letter ISO 4217 code*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAccount()
    {
        // Arrange
        var account = new Account("Old Name", "DE89370400440532013000", "EUR", 1000m);

        // Act
        account.Update("New Name", "FR1420041010050500013M02606");

        // Assert
        account.Name.Should().Be("New Name");
        account.IBAN.Should().Be("FR1420041010050500013M02606");
    }

    [Fact]
    public void HasSufficientBalance_WhenBalanceIsSufficient_ShouldReturnTrue()
    {
        // Arrange
        var account = new Account("Checking Account", "DE89370400440532013000", "EUR", 1000m);

        // Act & Assert
        account.HasSufficientBalance(-500m).Should().BeTrue();
        account.HasSufficientBalance(-1000m).Should().BeTrue();
    }

    [Fact]
    public void HasSufficientBalance_WhenBalanceIsInsufficient_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account("Checking Account", "DE89370400440532013000", "EUR", 500m);

        // Act & Assert
        account.HasSufficientBalance(-1000m).Should().BeFalse();
    }

    [Fact]
    public void UpdateBalance_ShouldCalculateFromTransactions()
    {
        // Arrange
        var account = new Account("Checking Account", "DE89370400440532013000", "EUR", 1000m);

        // Note: In real scenario, transactions would be added via repository
        // This is simplified for testing the calculation logic
        var transactions = new List<Transaction>
        {
            new Transaction(account.AccountId, 500m, "EUR", TransactionType.Deposit, "Deposit", DateTimeOffset.UtcNow),
            new Transaction(account.AccountId, -200m, "EUR", TransactionType.Withdrawal, "Withdrawal", DateTimeOffset.UtcNow)
        };

        // Use reflection to set the private Transactions collection for testing
        var transactionsProperty = account.GetType().GetProperty("Transactions");
        transactionsProperty!.SetValue(account, transactions);

        // Act
        account.UpdateBalance();

        // Assert
        account.CurrentBalance.Should().Be(1300m); // 1000 + 500 - 200
    }
}
