# Glossary - Terms and Definitions

## A

**Account**  
A financial account representing a bank account, cash wallet, credit card, or savings account. Contains transactions and has a balance in a specific currency.

**Account Type**  
Classification of accounts: Checking, Savings, Cash, Credit Card. Determines display and behavior.

**ADR (Architecture Decision Record)**  
Document capturing important architectural decisions, their context, and consequences.

**Aggregation Service**  
Service responsible for calculating totals across multiple accounts, potentially involving currency conversion.

**arc42**  
Template for architecture documentation. Provides a standardized structure for documenting software architecture.

## B

**Background Job**  
Automated task running independently of user requests. Example: `ExchangeRateUpdateJob` for daily exchange rate updates.

**Base Currency**  
The reference currency for exchange rate quotations. In this application: EUR (European Central Bank standard).

**Building Block**  
A component or module in the system architecture. Can be refined into sub-components.

## C

**Cache**  
Temporary storage of frequently accessed data to improve performance. Uses `IMemoryCache` with time-based expiration.

**Counter-Transaction**  
A linked pair of transactions representing a transfer between accounts. Example: Debit from Account A + Credit to Account B.

**Currency Code**  
ISO 4217 three-letter code representing a currency (e.g., EUR, USD, GBP, CHF).

**Currency Conversion**  
Process of converting an amount from one currency to another using an exchange rate.

**Currency Service**  
Service providing exchange rate lookup and currency conversion functionality.

## D

**DTO (Data Transfer Object)**  
Object used to transfer data between application layers, especially between API and client.

**Domain Layer**  
Central layer containing business logic, domain entities, and business rules. Independent of infrastructure.

## E

**ECB (European Central Bank)**  
Provider of official EUR exchange rates. Source for daily exchange rate data in this application.

**Entity**  
Domain object with a unique identity that persists over time. Examples: Account, Transaction, ExchangeRate.

**Exchange Rate**  
Ratio for converting one currency to another. Example: 1 EUR = 1.10 USD means rate is 1.10.

**ExchangeRateSource**  
Origin of exchange rate data: `ECB90Day` (recent 90 days) or `ECBHistorical` (complete archive since 1999).

## F

**FluentValidation**  
Library for building strongly-typed validation rules in .NET applications.

## G

**Gap Detection**  
Process of identifying missing exchange rate dates in the database for backfilling.

## H

**Hosted Service**  
Background service in ASP.NET Core running alongside the web application. Implements `IHostedService`.

## I

**IBAN (International Bank Account Number)**  
International standard for identifying bank accounts across national borders.

**ISO 4217**  
International standard for currency codes (e.g., EUR, USD, GBP).

## L

**Layered Architecture**  
Architectural pattern organizing code into layers: Presentation (API), Domain (Business Logic), Infrastructure (Data Access).

## M

**Migration**  
Entity Framework Core script for evolving database schema over time. Versioned and repeatable.

**Monorepo**  
Single repository containing multiple related projects (backend, frontend, docs).

## N

**ng-matero**  
Angular admin template based on Angular Material. Provides UI components and layout structure.

## P

**Precision**  
Number of decimal places for monetary values. Uses `decimal(18,6)` for exchange rates (e.g., 1.123456).

## R

**Repository Pattern**  
Design pattern for abstracting data access logic. Provides collection-like interface for domain entities.

**Retry Logic**  
Strategy for handling transient failures by automatically retrying operations with backoff delays.

## S

**Swagger**  
Tool for API documentation and testing. Generates interactive API documentation from code.

## T

**Target Currency**  
The currency to convert to in an exchange rate quotation. Example: In EUR→USD, USD is the target.

**Transaction**  
Financial transaction recording money movement. Has amount, currency, type (Income/Expense/Transfer), and date.

**Transaction Type**  
Classification of transactions: Income (money in), Expense (money out), Transfer (between accounts).

**Triangular Arbitrage**  
Method for converting between two currencies via a third intermediary currency (EUR in this application).  
Formula: `Rate(USD → GBP) = Rate(EUR → GBP) / Rate(EUR → USD)`

**TTL (Time To Live)**  
Duration for which cached data remains valid. Uses 1-hour TTL for exchange rates.

## U

**Unit of Work**  
Pattern for managing database transactions and ensuring consistency across multiple repository operations.

**Upsert**  
Database operation that inserts a new record or updates if it already exists.

## V

**Value Object**  
Immutable object defined by its values rather than identity. Example: Money (amount + currency).

## W

**Weekend Gap**  
Missing exchange rate data for Saturdays and Sundays (ECB does not publish on weekends).

## X

**XML (Extensible Markup Language)**  
Data format used by ECB for publishing exchange rate feeds. Parsed using `XDocument`.

---

*Last updated: 2025-11-09*
