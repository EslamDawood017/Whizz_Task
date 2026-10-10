using AutoMapper;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.Handlers;
using Employee.Domain.Entities;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using Employee.UnitTests.Common;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Features.Employee.Commands
{
    public class CreateEmployeeCommandHandlerTests : TestBase
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<ITenantContext> _mockTenantContext;
        private readonly Mock<ICustomDataValidator> _mockCustomDataValidator;
        private readonly CreateEmployeeCommandHandler _handler;
        private readonly Guid _tenantId = Guid.NewGuid();

        public CreateEmployeeCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockTenantContext = new Mock<ITenantContext>();
            _mockTenantContext.Setup(t => t.TenantId).Returns(_tenantId);

            _mockCustomDataValidator = new Mock<ICustomDataValidator>();
            _mockCustomDataValidator
                .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, null));

            _handler = new CreateEmployeeCommandHandler(
                _mockContext.Object,
                _mockTenantContext.Object,
                Mapper,
                _mockCustomDataValidator.Object);
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ShouldCreateEmployeeAndAssignCurrentTenant()
        {
            // Arrange
            var existingEmployees = new List<Domain.Entities.Employee>();
            var mockDbSet = existingEmployees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var command = new CreateEmployeeCommand
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                Salary = new Money(12000000, "USD")
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().NotBeNull();
            result.Data!.FirstName.Should().Be("Ada");
            result.Data.Email.Should().Be("ada@example.com");

            _mockContext.Verify(c => c.Employees.AddAsync(
                It.Is<Domain.Entities.Employee>(e => e.TenantId == _tenantId && e.Email == "ada@example.com"),
                It.IsAny<CancellationToken>()), Times.Once);

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyExistsInSameTenant_ShouldReturnConflictFailure()
        {
            // Arrange
            var existingEmployees = new List<Domain.Entities.Employee>
            {
                new Domain.Entities.Employee
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    Email = "duplicate@example.com",
                    FirstName = "Existing",
                    LastName = "User",
                    Department = "Support"
                }
            };

            var mockDbSet = existingEmployees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var command = new CreateEmployeeCommand
            {
                FirstName = "New",
                LastName = "User",
                Email = "DUPLICATE@example.com", // Case-insensitive test
                Department = "Sales",
                Status = EmployeeStatus.Active
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Error.Should().NotBeNull();
            result.Error.Should().Contain("already exists");
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenCustomDataValidationFails_ShouldReturnFailure()
        {
            // Arrange
            var existingEmployees = new List<Domain.Entities.Employee>();
            var mockDbSet = existingEmployees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            _mockCustomDataValidator
                .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, "Required custom field 'BadgeNumber' is missing."));

            var command = new CreateEmployeeCommand
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@example.com",
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                CustomData = "{}"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Error.Should().Be("Required custom field 'BadgeNumber' is missing.");
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
