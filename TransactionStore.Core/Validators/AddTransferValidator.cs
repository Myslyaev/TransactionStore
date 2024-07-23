using FluentValidation;
using TransactionStore.Core.Models.Requests;

namespace Backend.Core.Validators;

public class AddTransferValidator : AbstractValidator<TransferRequest>
{
    public AddTransferValidator()
    {
        RuleFor(t => t.AccountFromId)
            .NotEmpty().WithMessage("Enter a valid GUID.");

        RuleFor(t => t.AccountToId)
            .NotEmpty().WithMessage("Enter a valid GUID.");

        RuleFor(t => t.CurrencyFrom)
            .NotEmpty().WithMessage("The field cannot be empty. Specify the currency type.")
            .NotNull().WithMessage("The field cannot be null. Specify the currency type.");

        RuleFor(t => t.CurrencyTo)
            .NotEmpty().WithMessage("The field cannot be empty. Specify the currency type.")
            .NotNull().WithMessage("The field cannot be null. Specify the currency type.");

        RuleFor(t => t.Amount)
            .GreaterThanOrEqualTo(1).WithMessage("The amount should not be less than 1.")
            .LessThanOrEqualTo(1000000).WithMessage("The maximum limit for the operation has been exceeded.");
    }
}