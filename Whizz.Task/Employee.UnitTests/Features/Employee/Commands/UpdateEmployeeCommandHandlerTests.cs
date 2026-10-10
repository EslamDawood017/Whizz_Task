using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.Handlers;
using Employee.Domain.Entities;
using Employee.Domain.Enums;
using Employee.UnitTests.Common;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Features.Employee.Commands
{
    public class UpdateEmployeeCommandHandlerTests : TestBase
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<ICustomDataValidator> _mockCustomDataValidator;
        private readonly UpdateEmployeeCommandHandler _handler;

        public UpdateEmployeeCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockCustomDataValidator = new Mock<ICustomDataValidator>();
            _mockCustomDataValidator
                .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, null));

            _handler = new UpdateEmployeeCommandHandler(
                _mockContext.Object,
                Mapper,
                _mockCustomDataValidator.Object);
        }

        [Fact]
        public async Task Handle_WhenEmployeeExists_ShouldUpdateFieldsAndReturnSuccess()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var existingEmployee = new Domain.Entities.Employee
            {
                Id = employeeId,
                FirstName = "Alan",
                LastName = "Turing",
                Email = "alan@example.com",
                Department = "Computing",
                Status = EmployeeStatus.Active
            };

            var employees = new List<Domain.Entities.Employee> { existingEmployee };
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var command = new UpdateEmployeeCommand
            {
                Id = employeeId,
                FirstName = "Alan",
                LastName = "Turing",
                Email = "alan.turing@example.com",
                Department = "AI Lab",
                Status = EmployeeStatus.Active
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            existingEmployee.Department.Should().Be("AI Lab");
            existingEmployee.Email.Should().Be("alan.turing@example.com");
            existingEmployee.UpdatedAt.Should().NotBeNull();
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmployeeNotFound_ShouldReturnFailure()
        {
            // Arrange
            var employees = new List<Domain.Entities.Employee>();
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var command = new UpdateEmployeeCommand
            {
                Id = Guid.NewGuid(),
                FirstName = "Ghost",
                LastName = "User",
                Email = "ghost@example.com",
                Department = "None"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Error.Should().NotBeNull();
            result.Error.Should().Be("Employee not found.");
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
