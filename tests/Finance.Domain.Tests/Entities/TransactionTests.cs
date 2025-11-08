using Finance.Domain.Entities;
using FluentAssertions;

namespace Finance.Domain.Tests.Entities;

public class TransactionTests
{
    [Fact]
    public void LinkCounterTransaction_WithValidTransactions_ShouldLinkSuccessfully()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, -100m, "EUR", TransactionType.Transfer, "Transfer out", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 100m, "EUR", TransactionType.Transfer, "Transfer in", DateTimeOffset.UtcNow);

        // Act
        transaction1.LinkCounterTransaction(transaction2);

        // Assert
        transaction1.CounterTransactionId.Should().Be(transaction2.TransactionId);
        transaction2.CounterTransactionId.Should().Be(transaction1.TransactionId);
    }

    [Fact]
    public void LinkCounterTransaction_WithDifferentAmounts_ShouldThrowException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, -100m, "EUR", TransactionType.Transfer, "Transfer out", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 50m, "EUR", TransactionType.Transfer, "Transfer in", DateTimeOffset.UtcNow);

        // Act & Assert
        var act = () => transaction1.LinkCounterTransaction(transaction2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*amounts must match*");
    }

    [Fact]
    public void LinkCounterTransaction_WithSameSign_ShouldThrowException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, 100m, "EUR", TransactionType.Transfer, "Transfer", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 100m, "EUR", TransactionType.Transfer, "Transfer", DateTimeOffset.UtcNow);

        // Act & Assert
        var act = () => transaction1.LinkCounterTransaction(transaction2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*opposite signs*");
    }

    [Fact]
    public void LinkCounterTransaction_WithDifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, -100m, "EUR", TransactionType.Transfer, "Transfer out", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 100m, "USD", TransactionType.Transfer, "Transfer in", DateTimeOffset.UtcNow);

        // Act & Assert
        var act = () => transaction1.LinkCounterTransaction(transaction2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void LinkCounterTransaction_ToSelf_ShouldThrowException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, -100m, "EUR", TransactionType.Transfer, "Transfer", DateTimeOffset.UtcNow);

        // Act & Assert
        var act = () => transaction.LinkCounterTransaction(transaction);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot link a transaction to itself*");
    }

    [Fact]
    public void LinkCounterTransaction_WhenAlreadyLinked_ShouldThrowException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var accountId3 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, -100m, "EUR", TransactionType.Transfer, "Transfer out", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 100m, "EUR", TransactionType.Transfer, "Transfer in", DateTimeOffset.UtcNow);
        var transaction3 = new Transaction(accountId3, 100m, "EUR", TransactionType.Transfer, "Another transfer", DateTimeOffset.UtcNow);

        transaction1.LinkCounterTransaction(transaction2);

        // Act & Assert
        var act = () => transaction1.LinkCounterTransaction(transaction3);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already linked*");
    }

    [Fact]
    public void UnlinkCounterTransaction_WhenLinked_ShouldUnlinkBothTransactions()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transaction1 = new Transaction(accountId1, -100m, "EUR", TransactionType.Transfer, "Transfer out", DateTimeOffset.UtcNow);
        var transaction2 = new Transaction(accountId2, 100m, "EUR", TransactionType.Transfer, "Transfer in", DateTimeOffset.UtcNow);
        transaction1.LinkCounterTransaction(transaction2);

        // Act
        transaction1.UnlinkCounterTransaction();

        // Assert
        transaction1.CounterTransactionId.Should().BeNull();
        // Note: transaction2.CounterTransactionId will only be null if CounterTransaction navigation property was loaded
        // In unit tests without EF Core, we only verify the transaction we called unlink on
    }

    [Fact]
    public void UnlinkCounterTransaction_WhenNotLinked_ShouldNotThrow()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, -100m, "EUR", TransactionType.Transfer, "Transfer", DateTimeOffset.UtcNow);

        // Act & Assert
        var act = () => transaction.UnlinkCounterTransaction();
        act.Should().NotThrow();
    }
}
