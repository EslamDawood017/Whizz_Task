using Employee.Application.Features.Employee.Commands;
using FluentValidation;

namespace Employee.Application.Features.Employee.Validators;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid employee status.");

        When(x => x.Salary != null, () =>
        {
            RuleFor(x => x.Salary!.AmountMinor)
                .GreaterThanOrEqualTo(0).WithMessage("Salary amount cannot be negative.");

            RuleFor(x => x.Salary!.CurrencyCode)
                .NotEmpty().Length(3).WithMessage("Currency code must be 3 letters.");
        });
    }
}
