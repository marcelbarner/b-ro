# Transaction Creation and Linking

This scenario shows how a transfer between two accounts is implemented using counter-transactions.

## Scenario: Transfer Money Between Accounts

**Actors:**
- User
- Finance API
- Database

**Preconditions:**
- User is authenticated
- Two accounts exist (Account A and Account B)
- Account A has sufficient balance

**Flow:**

### 1. User Initiates Transfer

```
User → Frontend: Click "Transfer Money"
Frontend → User: Display transfer form
User → Frontend: Fill form (From: Account A, To: Account B, Amount: 100 EUR)
User → Frontend: Submit
```

### 2. Create Debit Transaction (Account A)

```
Frontend → API: POST /api/finance/accounts/{accountA}/transactions
{
  "amount": -100.00,
  "currency": "EUR",
  "type": "Transfer",
  "description": "Transfer to Account B",
  "date": "2025-11-08T10:00:00Z"
}

API → TransactionValidator: Validate request
TransactionValidator → API: Valid

API → TransactionService: CreateTransaction(accountA, -100, "Transfer", ...)
TransactionService → AccountRepository: GetAccount(accountA)
AccountRepository → Database: SELECT * FROM Accounts WHERE Id = accountA
Database → AccountRepository: Account data
AccountRepository → TransactionService: Account A

TransactionService: Validate sufficient balance (CurrentBalance >= 100)
TransactionService → TransactionRepository: CreateTransaction(transaction1)
TransactionRepository → Database: INSERT INTO Transactions ...
Database → TransactionRepository: Transaction1 created (ID: trans1)
```

### 3. Create Credit Transaction (Account B)

```
TransactionService → AccountRepository: GetAccount(accountB)
AccountRepository → Database: SELECT * FROM Accounts WHERE Id = accountB
Database → AccountRepository: Account data
AccountRepository → TransactionService: Account B

TransactionService → TransactionRepository: CreateTransaction(transaction2)
{
  "accountId": accountB,
  "amount": 100.00,
  "currency": "EUR",
  "type": "Transfer",
  "description": "Transfer from Account A"
}
TransactionRepository → Database: INSERT INTO Transactions ...
Database → TransactionRepository: Transaction2 created (ID: trans2)
```

### 4. Link Counter-Transactions

```
TransactionService: LinkCounterTransactions(trans1, trans2)
TransactionService → Transaction1: SetCounterTransaction(trans2)
TransactionService → Transaction2: SetCounterTransaction(trans1)
TransactionRepository → Database: UPDATE Transactions SET CounterTransactionId = trans2 WHERE Id = trans1
TransactionRepository → Database: UPDATE Transactions SET CounterTransactionId = trans1 WHERE Id = trans2
```

### 5. Update Account Balances

```
TransactionService → Account A: UpdateBalance(-100)
Account A: CurrentBalance = InitialBalance + SUM(transactions)
TransactionService → AccountRepository: UpdateAccount(accountA)
AccountRepository → Database: UPDATE Accounts SET CurrentBalance = ... WHERE Id = accountA

TransactionService → Account B: UpdateBalance(+100)
Account B: CurrentBalance = InitialBalance + SUM(transactions)
TransactionService → AccountRepository: UpdateAccount(accountB)
AccountRepository → Database: UPDATE Accounts SET CurrentBalance = ... WHERE Id = accountB
```

### 6. Commit Transaction

```
TransactionService → UnitOfWork: Commit()
UnitOfWork → Database: COMMIT TRANSACTION
Database → UnitOfWork: Success
```

### 7. Return Response

```
API → Frontend: 201 Created
{
  "transactionId": "trans1",
  "counterTransactionId": "trans2",
  "accountABalance": 900.00,
  "accountBBalance": 1100.00
}

Frontend → User: Display success message
Frontend: Update account balances in UI
```

## Sequence Diagram

```
┌────┐          ┌─────────┐      ┌─────┐      ┌──────────┐      ┌──────────┐      ┌──────┐
│User│          │Frontend │      │ API │      │ Service  │      │Repository│      │  DB  │
└─┬──┘          └────┬────┘      └──┬──┘      └────┬─────┘      └────┬─────┘      └───┬──┘
  │                  │               │              │                 │                │
  │ Transfer Request │               │              │                 │                │
  ├─────────────────>│               │              │                 │                │
  │                  │               │              │                 │                │
  │                  │ POST /transactions           │                 │                │
  │                  ├──────────────>│              │                 │                │
  │                  │               │              │                 │                │
  │                  │               │ CreateTransaction(A, -100)     │                │
  │                  │               ├─────────────>│                 │                │
  │                  │               │              │                 │                │
  │                  │               │              │ GetAccount(A)   │                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ SELECT         │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │              │<────────────────┤                │
  │                  │               │              │                 │                │
  │                  │               │              │ CreateTrans(A)  │                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ INSERT         │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │              │<────────────────┤                │
  │                  │               │              │                 │                │
  │                  │               │              │ CreateTrans(B)  │                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ INSERT         │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │              │<────────────────┤                │
  │                  │               │              │                 │                │
  │                  │               │              │ LinkCounterTrans│                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ UPDATE (2x)    │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │              │<────────────────┤                │
  │                  │               │              │                 │                │
  │                  │               │              │ UpdateBalance   │                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ UPDATE (2x)    │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │              │<────────────────┤                │
  │                  │               │              │                 │                │
  │                  │               │              │ Commit()        │                │
  │                  │               │              ├────────────────>│                │
  │                  │               │              │                 │ COMMIT         │
  │                  │               │              │                 ├───────────────>│
  │                  │               │              │                 │<───────────────┤
  │                  │               │<─────────────┤                 │                │
  │                  │               │              │                 │                │
  │                  │<──────────────┤              │                 │                │
  │                  │ 201 Created   │              │                 │                │
  │<─────────────────┤               │              │                 │                │
  │ Success          │               │              │                 │                │
```

## Alternative Flow: Insufficient Balance

If Account A has insufficient balance:

```
TransactionService: Validate sufficient balance
TransactionService: FAIL - Balance too low
TransactionService → API: Throw InsufficientBalanceException
API → Frontend: 400 Bad Request
{
  "error": "Insufficient balance",
  "currentBalance": 50.00,
  "requiredAmount": 100.00
}
Frontend → User: Display error message
```

## Alternative Flow: Currency Mismatch

If accounts have different currencies:

```
TransactionService: Validate currencies match
TransactionService: FAIL - Currency mismatch
TransactionService → API: Throw CurrencyMismatchException
API → Frontend: 400 Bad Request
{
  "error": "Currency mismatch",
  "accountACurrency": "EUR",
  "accountBCurrency": "USD"
}
Frontend → User: Display error message
```

## Delete Counter-Transaction Flow

When deleting a transaction that has a counter-transaction:

```
User → Frontend: Delete transaction
Frontend → API: DELETE /api/finance/transactions/{trans1}

API → TransactionService: DeleteTransaction(trans1)
TransactionService → TransactionRepository: GetTransaction(trans1)
TransactionRepository → TransactionService: Transaction (with CounterTransactionId = trans2)

TransactionService: Check if has counter-transaction
TransactionService → UnitOfWork: BeginTransaction()

TransactionService → Transaction1: UnlinkCounterTransaction()
TransactionService → Transaction2: UnlinkCounterTransaction()
TransactionRepository → Database: UPDATE Transactions SET CounterTransactionId = NULL WHERE Id IN (trans1, trans2)

TransactionService → TransactionRepository: DeleteTransaction(trans1)
TransactionRepository → Database: DELETE FROM Transactions WHERE Id = trans1

TransactionService → Account: UpdateBalance(+100)  // Reverse the debit
TransactionRepository → Database: UPDATE Accounts ...

TransactionService → UnitOfWork: Commit()
```

**Note:** Both transactions must be unlinked before deletion. The counter-transaction (trans2) remains in the system but is no longer linked.

## Business Rules Enforced

1. **Counter-transactions must have matching amounts** (absolute value)
2. **Counter-transactions must have the same currency**
3. **A transaction can only have one counter-transaction**
4. **Deleting a transaction unlinks its counter-transaction first**
5. **Account balance is recalculated after every transaction change**
6. **All operations within a transfer are atomic** (database transaction)

## Error Handling

- Validation errors return 400 Bad Request
- Business rule violations return 422 Unprocessable Entity
- Database errors trigger rollback and return 500 Internal Server Error
- All errors are logged with correlation IDs

## Performance Considerations

- Single database round-trip for balance calculation (using aggregation)
- Optimistic concurrency control to prevent race conditions
- Database transactions keep locks minimal (short-lived)
- Indexes on AccountId and Date for fast transaction queries
