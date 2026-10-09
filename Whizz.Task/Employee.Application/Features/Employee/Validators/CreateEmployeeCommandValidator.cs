using Employee.Application.Features.Employee.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Validators
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required.")
                .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be a valid employee status.");

        
            When(x => x.Salary != null, () =>
            {
                RuleFor(x => x.Salary!.AmountMinor)
                    .GreaterThanOrEqualTo(0).WithMessage("Salary amount cannot be negative.");

                RuleFor(x => x.Salary!.CurrencyCode)
                    .NotEmpty().WithMessage("Currency code is required.")
                    .Length(3).WithMessage("Currency code must be a 3-letter ISO code (e.g. USD).");
            });
        }
    }
}
