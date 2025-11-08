using Finance.API.DTOs;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers;

/// <summary>
/// Controller for managing transactions and transfers.
/// </summary>
[ApiController]
[Route("api/finance")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILogger<TransactionsController> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all transactions for an account.
    /// </summary>
    [HttpGet("accounts/{accountId:guid}/transactions")]
    [ProducesResponseType<IEnumerable<TransactionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
                return NotFound($"Account with ID {accountId} not found.");

            var transactions = await _transactionRepository.GetByAccountIdAsync(accountId, cancellationToken);
            var transactionDtos = transactions.Select(MapToDto);
            return Ok(transactionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving transactions.");
        }
    }

    /// <summary>
    /// Gets a transaction by ID.
    /// </summary>
    [HttpGet("transactions/{id:guid}")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionRepository.GetWithCounterTransactionAsync(id, cancellationToken);
            if (transaction == null)
                return NotFound($"Transaction with ID {id} not found.");

            return Ok(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, "An error occurred while retrieving the transaction.");
        }
    }

    /// <summary>
    /// Creates a new transaction for an account.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/transactions")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        Guid accountId,
        [FromBody] CreateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
                return NotFound($"Account with ID {accountId} not found.");

            // Validate currency matches account
            if (dto.Currency != account.Currency)
            {
                return BadRequest($"Transaction currency ({dto.Currency}) must match account currency ({account.Currency}).");
            }

            // Check sufficient balance for debits
            if (!account.HasSufficientBalance(dto.Amount))
            {
                return BadRequest($"Insufficient balance. Current: {account.CurrentBalance}, Required: {account.CurrentBalance + dto.Amount}");
            }

            var transaction = new Transaction(
                accountId,
                dto.Amount,
                dto.Currency,
                dto.Type,
                dto.Description,
                dto.Date ?? DateTimeOffset.UtcNow
            );

            await _transactionRepository.CreateAsync(transaction, cancellationToken);

            // Update account balance
            account.UpdateBalance();
            await _accountRepository.UpdateAsync(account, cancellationToken);

            _logger.LogInformation("Created transaction {TransactionId} for account {AccountId}", transaction.TransactionId, accountId);

            var transactionDto = MapToDto(transaction);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transactionDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for account {AccountId}", accountId);
            return StatusCode(500, "An error occurred while creating the transaction.");
        }
    }

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    [HttpDelete("transactions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                return NotFound($"Transaction with ID {id} not found.");

            var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);

            await _transactionRepository.DeleteAsync(id, cancellationToken);

            // Update account balance
            if (account != null)
            {
                account.UpdateBalance();
                await _accountRepository.UpdateAsync(account, cancellationToken);
            }

            _logger.LogInformation("Deleted transaction {TransactionId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
            return StatusCode(500, "An error occurred while deleting the transaction.");
        }
    }

    /// <summary>
    /// Links two transactions as counter-transactions.
    /// </summary>
    [HttpPost("transactions/{id:guid}/link-counter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LinkCounterTransaction(
        Guid id,
        [FromBody] LinkCounterTransactionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                return NotFound($"Transaction with ID {id} not found.");

            var counterTransaction = await _transactionRepository.GetByIdAsync(dto.CounterTransactionId, cancellationToken);
            if (counterTransaction == null)
                return NotFound($"Counter-transaction with ID {dto.CounterTransactionId} not found.");

            transaction.LinkCounterTransaction(counterTransaction);

            await _transactionRepository.UpdateAsync(transaction, cancellationToken);
            await _transactionRepository.UpdateAsync(counterTransaction, cancellationToken);

            _logger.LogInformation("Linked transaction {TransactionId} with counter-transaction {CounterTransactionId}",
                id, dto.CounterTransactionId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking counter-transaction");
            return StatusCode(500, "An error occurred while linking the counter-transaction.");
        }
    }

    /// <summary>
    /// Unlinks a transaction from its counter-transaction.
    /// </summary>
    [HttpDelete("transactions/{id:guid}/link-counter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkCounterTransaction(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionRepository.GetWithCounterTransactionAsync(id, cancellationToken);
            if (transaction == null)
                return NotFound($"Transaction with ID {id} not found.");

            transaction.UnlinkCounterTransaction();
            await _transactionRepository.UpdateAsync(transaction, cancellationToken);

            _logger.LogInformation("Unlinked counter-transaction from {TransactionId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking counter-transaction");
            return StatusCode(500, "An error occurred while unlinking the counter-transaction.");
        }
    }

    /// <summary>
    /// Creates a transfer between two accounts (creates two linked transactions).
    /// </summary>
    [HttpPost("transfers")]
    [ProducesResponseType<TransferResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferResultDto>> CreateTransfer(
        [FromBody] CreateTransferDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var fromAccount = await _accountRepository.GetByIdAsync(dto.FromAccountId, cancellationToken);
            if (fromAccount == null)
                return NotFound($"Source account with ID {dto.FromAccountId} not found.");

            var toAccount = await _accountRepository.GetByIdAsync(dto.ToAccountId, cancellationToken);
            if (toAccount == null)
                return NotFound($"Destination account with ID {dto.ToAccountId} not found.");

            // Validate currencies match
            if (fromAccount.Currency != toAccount.Currency)
            {
                return BadRequest($"Currency mismatch: {fromAccount.Currency} != {toAccount.Currency}");
            }

            // Check sufficient balance
            if (!fromAccount.HasSufficientBalance(-dto.Amount))
            {
                return BadRequest($"Insufficient balance in source account. Current: {fromAccount.CurrentBalance}, Required: {dto.Amount}");
            }

            // Create debit transaction (source account)
            var debitTransaction = new Transaction(
                dto.FromAccountId,
                -dto.Amount,
                fromAccount.Currency,
                TransactionType.Transfer,
                $"{dto.Description} (to {toAccount.Name})",
                DateTimeOffset.UtcNow
            );

            // Create credit transaction (destination account)
            var creditTransaction = new Transaction(
                dto.ToAccountId,
                dto.Amount,
                toAccount.Currency,
                TransactionType.Transfer,
                $"{dto.Description} (from {fromAccount.Name})",
                DateTimeOffset.UtcNow
            );

            // Link as counter-transactions
            debitTransaction.LinkCounterTransaction(creditTransaction);

            await _transactionRepository.CreateAsync(debitTransaction, cancellationToken);
            await _transactionRepository.CreateAsync(creditTransaction, cancellationToken);

            // Update balances
            fromAccount.UpdateBalance();
            toAccount.UpdateBalance();
            await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
            await _accountRepository.UpdateAsync(toAccount, cancellationToken);

            _logger.LogInformation("Created transfer from account {FromAccountId} to {ToAccountId}, amount {Amount}",
                dto.FromAccountId, dto.ToAccountId, dto.Amount);

            var result = new TransferResultDto(
                debitTransaction.TransactionId,
                creditTransaction.TransactionId,
                fromAccount.CurrentBalance,
                toAccount.CurrentBalance
            );

            return CreatedAtAction(nameof(GetTransaction), new { id = debitTransaction.TransactionId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transfer");
            return StatusCode(500, "An error occurred while creating the transfer.");
        }
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.TransactionId,
            transaction.AccountId,
            transaction.Amount,
            transaction.Currency,
            transaction.Type,
            transaction.Description,
            transaction.Date,
            transaction.CounterTransactionId,
            transaction.CreatedAt,
            transaction.UpdatedAt
        );
    }
}
