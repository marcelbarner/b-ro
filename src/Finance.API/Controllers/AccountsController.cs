using Finance.API.DTOs;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers;

/// <summary>
/// Controller for managing bank accounts.
/// </summary>
[ApiController]
[Route("api/finance/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IAccountRepository accountRepository,
        ILogger<AccountsController> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all accounts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<AccountDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts(CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await _accountRepository.GetAllAsync(cancellationToken);
            var accountDtos = accounts.Select(MapToDto);
            return Ok(accountDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(500, "An error occurred while retrieving accounts.");
        }
    }

    /// <summary>
    /// Gets an account by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
                return NotFound($"Account with ID {id} not found.");

            return Ok(MapToDto(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", id);
            return StatusCode(500, "An error occurred while retrieving the account.");
        }
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> CreateAccount(
        [FromBody] CreateAccountDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if IBAN already exists
            if (await _accountRepository.ExistsWithIbanAsync(dto.IBAN, cancellationToken))
            {
                return BadRequest($"An account with IBAN {dto.IBAN} already exists.");
            }

            var account = new Account(dto.Name, dto.IBAN, dto.Currency, dto.InitialBalance);
            await _accountRepository.CreateAsync(account, cancellationToken);

            _logger.LogInformation("Created account {AccountId} with name {Name}", account.AccountId, account.Name);

            var accountDto = MapToDto(account);
            return CreatedAtAction(nameof(GetAccount), new { id = account.AccountId }, accountDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, "An error occurred while creating the account.");
        }
    }

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccount(
        Guid id,
        [FromBody] UpdateAccountDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
                return NotFound($"Account with ID {id} not found.");

            account.Update(dto.Name, dto.IBAN);
            await _accountRepository.UpdateAsync(account, cancellationToken);

            _logger.LogInformation("Updated account {AccountId}", id);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {AccountId}", id);
            return StatusCode(500, "An error occurred while updating the account.");
        }
    }

    /// <summary>
    /// Deletes an account.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
                return NotFound($"Account with ID {id} not found.");

            // Check if account has transactions
            if (account.Transactions.Any())
            {
                return BadRequest("Cannot delete account with existing transactions. Delete transactions first.");
            }

            await _accountRepository.DeleteAsync(id, cancellationToken);

            _logger.LogInformation("Deleted account {AccountId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {AccountId}", id);
            return StatusCode(500, "An error occurred while deleting the account.");
        }
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto(
            account.AccountId,
            account.Name,
            account.IBAN,
            account.Currency,
            account.InitialBalance,
            account.CurrentBalance,
            account.CreatedAt,
            account.UpdatedAt
        );
    }
}
