using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.Handlers;
using Employee.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Features.Employee.Commands
{
    public class DeleteEmployeeCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly DeleteEmployeeCommandHandler _handler;

        public DeleteEmployeeCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new DeleteEmployeeCommandHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_WhenEmployeeExists_ShouldSetDeletedAtSoftDelete()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Domain.Entities.Employee
            {
                Id = employeeId,
                FirstName = "Margaret",
                LastName = "Hamilton",
                Email = "margaret@example.com",
                DeletedAt = null
            };

            var employees = new List<Domain.Entities.Employee> { employee };
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(new DeleteEmployeeCommand(employeeId), CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().BeTrue();
            employee.DeletedAt.Should().NotBeNull();
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmployeeNotFound_ShouldReturnFailure()
        {
            // Arrange
            var employees = new List<Domain.Entities.Employee>();
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(new DeleteEmployeeCommand(Guid.NewGuid()), CancellationToken.None);

            // Assert
            result.Error.Should().NotBeNull();
            result.Error.Should().Be("Employee not found.");
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
