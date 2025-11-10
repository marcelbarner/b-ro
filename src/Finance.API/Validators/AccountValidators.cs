using System.Text.RegularExpressions;
using Finance.API.DTOs;
using FluentValidation;

namespace Finance.API.Validators;

/// <summary>
/// Validator for CreateAccountDto.
/// </summary>
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    private static readonly Regex IbanRegex = new(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$", RegexOptions.Compiled);

    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(100).WithMessage("Account name cannot exceed 100 characters.");

        RuleFor(x => x.IBAN)
            .NotEmpty().WithMessage("IBAN is required.")
            .MaximumLength(34).WithMessage("IBAN cannot exceed 34 characters.")
            .Matches(IbanRegex).WithMessage("IBAN format is invalid. Must start with 2 letters, followed by 2 digits.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO 4217 code (e.g., EUR, USD).")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be uppercase letters.");
    }
}

/// <summary>
/// Validator for UpdateAccountDto.
/// </summary>
public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    private static readonly Regex IbanRegex = new(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$", RegexOptions.Compiled);

    public UpdateAccountDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(100).WithMessage("Account name cannot exceed 100 characters.");

        RuleFor(x => x.IBAN)
            .NotEmpty().WithMessage("IBAN is required.")
            .MaximumLength(34).WithMessage("IBAN cannot exceed 34 characters.")
            .Matches(IbanRegex).WithMessage("IBAN format is invalid.");
    }
}
