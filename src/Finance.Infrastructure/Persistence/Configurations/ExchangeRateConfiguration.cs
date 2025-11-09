using Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the ExchangeRate entity.
/// </summary>
public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.BaseCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .HasDefaultValue("EUR");

        builder.Property(e => e.TargetCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(e => e.Rate)
            .IsRequired()
            .HasPrecision(18, 6); // Higher precision for exchange rates

        builder.Property(e => e.Source)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Composite index for fast lookups by date and target currency
        builder.HasIndex(e => new { e.Date, e.TargetCurrency })
            .IsUnique(); // Each currency has one rate per date

        // Index for source-based queries
        builder.HasIndex(e => e.Source);
    }
}
