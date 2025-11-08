# Finance Domain - Building Blocks

## Overview

The Finance domain implements bank account and transaction management functionality. It follows a layered architecture with clear separation of concerns.

## Component Structure

```
Finance Domain
├── Finance.API          (Presentation Layer)
│   ├── Controllers
│   ├── DTOs
│   └── Validators
├── Finance.Domain       (Domain Layer)
│   ├── Entities
│   ├── ValueObjects
│   ├── Interfaces
│   └── Services
└── Finance.Infrastructure (Infrastructure Layer)
    ├── Data
    ├── Repositories
    └── Configurations
```

## Finance.API (Presentation Layer)

### Responsibility
Exposes RESTful API endpoints for account and transaction management. Handles HTTP requests, validation, and response formatting.

### Key Components

#### Controllers
- **AccountsController**: Manages bank account CRUD operations
  - `GET /api/finance/accounts` - List all active accounts
  - `POST /api/finance/accounts` - Create new account
  - `GET /api/finance/accounts/{id}` - Get account details
  - `PUT /api/finance/accounts/{id}` - Update account
  - `PATCH /api/finance/accounts/{id}/archive` - Archive account
  - `DELETE /api/finance/accounts/{id}` - Delete account (if no transactions)
  
- **TransactionsController**: Manages financial transactions
  - `GET /api/finance/accounts/{accountId}/transactions` - List account transactions
  - `POST /api/finance/accounts/{accountId}/transactions` - Create transaction
  - `GET /api/finance/transactions/{id}` - Get transaction details
  - `PUT /api/finance/transactions/{id}` - Update transaction
  - `DELETE /api/finance/transactions/{id}` - Delete transaction
  - `POST /api/finance/transactions/{id}/link-counter` - Link counter-transaction
  - `DELETE /api/finance/transactions/{id}/unlink-counter` - Unlink counter-transaction

#### DTOs (Data Transfer Objects)
- `AccountDto`: Account data for API responses
- `CreateAccountRequest`: Request payload for account creation
- `UpdateAccountRequest`: Request payload for account updates
- `TransactionDto`: Transaction data for API responses
- `CreateTransactionRequest`: Request payload for transaction creation
- `UpdateTransactionRequest`: Request payload for transaction updates
- `LinkCounterTransactionRequest`: Request payload for linking counter-transactions

#### Validators
- `AccountValidator`: Validates account data (FluentValidation)
- `TransactionValidator`: Validates transaction data and business rules

### Dependencies
- Finance.Domain (for domain models and services)
- Microsoft.AspNetCore.Mvc
- FluentValidation.AspNetCore
- AutoMapper (for DTO mapping)

## Finance.Domain (Domain Layer)

### Responsibility
Contains business logic, domain entities, and interfaces. Implements core financial rules and validations.

### Key Components

#### Entities

**Account**
```csharp
public class Account
{
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? BankName { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; private set; }
    public bool IsArchived { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Transaction> Transactions { get; set; }
    
    // Business methods
    public void UpdateBalance(decimal amount);
    public bool CanBeDeleted();
}
```

**Transaction**
```csharp
public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public TransactionType Type { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? CounterTransactionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation
    public Account Account { get; set; }
    public Category? Category { get; set; }
    public Transaction? CounterTransaction { get; set; }
    
    // Business methods
    public void LinkCounterTransaction(Transaction other);
    public void UnlinkCounterTransaction();
    public bool CanBe Linked(Transaction other);
}
```

#### Value Objects
- `Money`: Encapsulates amount and currency
- `IBAN`: Value object with validation

#### Enums
- `AccountType`: Checking, Savings, Cash, CreditCard
- `TransactionType`: Income, Expense, Transfer

#### Interfaces
- `IAccountRepository`: Repository interface for accounts
- `ITransactionRepository`: Repository interface for transactions
- `IUnitOfWork`: Unit of work pattern for transaction management

#### Domain Services
- `AccountService`: Business logic for account operations
  - Validate account creation rules
  - Handle account archival logic
  - Ensure deletion constraints
  
- `TransactionService`: Business logic for transactions
  - Calculate balance impacts
  - Validate counter-transaction linking
  - Enforce consistency rules

### Dependencies
- None (pure domain layer, no external dependencies)

## Finance.Infrastructure (Infrastructure Layer)

### Responsibility
Implements data persistence, database access, and external integrations. Uses Entity Framework Core for data access.

### Key Components

#### Data Context
- **FinanceDbContext**: EF Core database context
  - DbSet<Account> Accounts
  - DbSet<Transaction> Transactions
  - DbSet<Category> Categories
  - Configuration via Fluent API

#### Repositories
- `AccountRepository`: Implements `IAccountRepository`
  - CRUD operations
  - Query accounts with filters
  - Include related transactions
  
- `TransactionRepository`: Implements `ITransactionRepository`
  - CRUD operations
  - Query transactions with filters
  - Handle counter-transaction relationships

#### Entity Configurations
- `AccountConfiguration`: EF Core entity configuration
  - Table mapping
  - Property constraints
  - Relationships
  
- `TransactionConfiguration`: EF Core entity configuration
  - Self-referencing relationship for counter-transactions
  - Indexes for performance

#### Migrations
- Database schema migrations
- Seed data for testing/development

### Dependencies
- Finance.Domain (for domain models and interfaces)
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL (or SQL Server provider)

## Component Interaction

### Account Creation Flow
```
Client Request
    ↓
AccountsController (API)
    ↓ Validate DTO
AccountValidator
    ↓ Map to Domain
AccountService (Domain)
    ↓ Business Logic
AccountRepository (Infrastructure)
    ↓ Persist
Database
```

### Transaction with Counter-Transaction Flow
```
Client Request (Create Transfer)
    ↓
TransactionsController (API)
    ↓ Validate
TransactionValidator
    ↓ Create Transaction 1 (Debit)
TransactionService (Domain)
    ↓
TransactionRepository (Infrastructure)
    ↓ Create Transaction 2 (Credit)
TransactionService (Domain)
    ↓ Link Both Transactions
TransactionService (Domain)
    ↓ Update Account Balances
AccountService (Domain)
    ↓
Database (Unit of Work Commit)
```

## Cross-Cutting Concerns

### Error Handling
- Domain exceptions for business rule violations
- API exception middleware for consistent error responses
- Validation exceptions from FluentValidation

### Logging
- Structured logging via ILogger
- Log account operations (create, update, archive, delete)
- Log transaction operations with balance changes
- Track counter-transaction linking/unlinking

### Validation
- DTO validation at API layer (FluentValidation)
- Business rule validation at domain layer
- Database constraints at infrastructure layer

### Transaction Management
- Unit of Work pattern for atomic operations
- Database transactions for counter-transaction creation
- Rollback on failure

## Dependencies Between Layers

```
Finance.API
    ↓ depends on
Finance.Domain
    ↑ interfaces
Finance.Infrastructure
```

- API layer depends on Domain (and transitively on Infrastructure via DI)
- Infrastructure implements Domain interfaces
- Domain has no dependencies (clean architecture)

## Deployment

The Finance domain is part of the Finance.API service, which runs as a Docker container. The database connection is configured via environment variables.

```yaml
services:
  finance-api:
    build: ./src/Finance.API
    environment:
      - ConnectionStrings__FinanceDb=...
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5001:8080"
    depends_on:
      - postgres
```

## Security

- Authentication: JWT tokens required for all endpoints
- Authorization: Role-based access (all users can manage their own accounts)
- Data protection: Sensitive data (account numbers, IBAN) encrypted at rest
- Input validation: All inputs validated before processing

## Performance Considerations

- Database indexes on frequently queried fields (AccountId, Date, Type)
- Pagination for transaction lists
- Lazy loading disabled, explicit includes used
- Caching for account balances (with invalidation on transaction changes)

## Testing Strategy

- **Unit Tests**: Domain services, validators, business rules
- **Integration Tests**: Repositories, database operations
- **API Tests**: Controller endpoints, request/response validation
- **End-to-End Tests**: Complete workflows (account creation, transfer scenarios)
