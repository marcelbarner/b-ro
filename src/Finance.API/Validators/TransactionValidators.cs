using Finance.API.DTOs;
using FluentValidation;

namespace Finance.API.Validators;

/// <summary>
/// Validator for CreateTransactionDto.
/// </summary>
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO 4217 code.")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be uppercase letters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Date)
            .Must(date => !date.HasValue || date.Value <= DateTimeOffset.UtcNow)
            .WithMessage("Transaction date cannot be in the future.");
    }
}

/// <summary>
/// Validator for CreateTransferDto.
/// </summary>
public class CreateTransferDtoValidator : AbstractValidator<CreateTransferDto>
{
    public CreateTransferDtoValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Source account is required.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("Destination account is required.")
            .NotEqual(x => x.FromAccountId).WithMessage("Source and destination accounts cannot be the same.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

/// <summary>
/// Validator for LinkCounterTransactionDto.
/// </summary>
public class LinkCounterTransactionDtoValidator : AbstractValidator<LinkCounterTransactionDto>
{
    public LinkCounterTransactionDtoValidator()
    {
        RuleFor(x => x.CounterTransactionId)
            .NotEmpty().WithMessage("Counter-transaction ID is required.");
    }
}
