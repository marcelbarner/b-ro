using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCurrencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Date",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Transactions",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "CounterTransactionId",
                table: "Transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateToEUR",
                table: "Transactions",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialBalance",
                table: "Accounts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "Accounts",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 34);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBalance",
                table: "Accounts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "Accounts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    BaseCurrency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "EUR"),
                    TargetCurrency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Date_TargetCurrency",
                table: "ExchangeRates",
                columns: new[] { "Date", "TargetCurrency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Source",
                table: "ExchangeRates",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "ExchangeRateToEUR",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Date",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Transactions",
                type: "TEXT",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "CounterTransactionId",
                table: "Transactions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Amount",
                table: "Transactions",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Accounts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "InitialBalance",
                table: "Accounts",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "Accounts",
                type: "TEXT",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentBalance",
                table: "Accounts",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "TEXT",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
