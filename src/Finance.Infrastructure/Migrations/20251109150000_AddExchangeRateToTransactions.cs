using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateToEUR",
                table: "Transactions",
                type: "TEXT",
                precision: 18,
                scale: 6,
                nullable: true);

            // Set default exchange rate to NULL for EUR transactions
            // For non-EUR transactions created before this migration, we cannot
            // retroactively determine the historical exchange rate, so we leave it as NULL
            migrationBuilder.Sql(
                @"UPDATE Transactions
                  SET ExchangeRateToEUR = NULL
                  WHERE Currency = 'EUR' OR ExchangeRateToEUR IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRateToEUR",
                table: "Transactions");
        }
    }
}
