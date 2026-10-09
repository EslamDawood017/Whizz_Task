using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.Validators;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace Employee.UnitTests.Features.Employee.Validators
{
    public class CreateEmployeeCommandValidatorTests
    {
        private readonly CreateEmployeeCommandValidator _validator = new();

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyErrors()
        {
            var command = new CreateEmployeeCommand
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                Salary = new Money(500000, "USD")
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_WhenFirstNameIsEmpty_ShouldHaveValidationError(string? invalidName)
        {
            var command = new CreateEmployeeCommand { FirstName = invalidName! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("user@")]
        public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError(string invalidEmail)
        {
            var command = new CreateEmployeeCommand { Email = invalidEmail };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_WhenSalaryAmountIsNegative_ShouldHaveValidationError()
        {
            var command = new CreateEmployeeCommand
            {
                Salary = new Money(-500, "USD")
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Salary!.AmountMinor);
        }
    }
}
