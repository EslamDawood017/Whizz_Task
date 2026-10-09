using Employee.Application.Features.Employee.Commands;
using FluentValidation;

namespace Employee.Application.Features.Employee.Validators;

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("A valid employee ID is required.");
    }
}
