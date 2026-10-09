using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Handlers;
using Employee.Application.Features.Employee.Queries;
using Employee.Domain.Entities;
using Employee.UnitTests.Common;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Features.Employee.Queries
{
    public class GetEmployeeByIdQueryHandlerTests : TestBase
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetEmployeeByIdQueryHandler _handler;

        public GetEmployeeByIdQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetEmployeeByIdQueryHandler(_mockContext.Object, Mapper);
        }

        [Fact]
        public async Task Handle_WhenEmployeeExists_ShouldReturnEmployeeDto()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employees = new List<Domain.Entities.Employee>
            {
                new Domain.Entities.Employee
                {
                    Id = employeeId,
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Email = "grace@example.com",
                    Department = "Research"
                }
            };

            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(new GetEmployeeByIdQuery(employeeId), CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(employeeId);
            result.Data.FirstName.Should().Be("Grace");
        }

        [Fact]
        public async Task Handle_WhenEmployeeNotFound_ShouldReturnFailure()
        {
            // Arrange
            var employees = new List<Domain.Entities.Employee>();
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(new GetEmployeeByIdQuery(Guid.NewGuid()), CancellationToken.None);

            // Assert
            result.Error.Should().NotBeNull();
            result.Error.Should().Be("Employee not found.");
        }
    }
}
