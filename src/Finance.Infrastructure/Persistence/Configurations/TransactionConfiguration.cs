using Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Transaction entity.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.AccountId)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(20);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.CounterTransactionId);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.Property(t => t.ExchangeRateToEUR)
            .HasPrecision(18, 6); // Higher precision for exchange rates

        // Computed column - not mapped to database
        builder.Ignore(t => t.ConvertedAmountEUR);

        // Index for efficient queries
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.Date);
        builder.HasIndex(t => t.CounterTransactionId);

        // Self-referencing relationship for counter-transactions
        builder.HasOne(t => t.CounterTransaction)
            .WithMany()
            .HasForeignKey(t => t.CounterTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
