using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Handlers;
using Employee.Application.Features.Employee.Queries;
using Employee.Domain.Entities;
using Employee.Domain.Enums;
using Employee.UnitTests.Common;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Features.Employee.Queries
{
    public class GetEmployeesQueryHandlerTests : TestBase
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetEmployeesQueryHandler _handler;

        public GetEmployeesQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetEmployeesQueryHandler(_mockContext.Object, Mapper);
        }

        private List<Domain.Entities.Employee> GenerateEmployees(int count)
        {
            return Enumerable.Range(1, count).Select(i => new Domain.Entities.Employee
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@example.com",
                Department = i % 2 == 0 ? "Engineering" : "HR",
                Status = i % 3 == 0 ? EmployeeStatus.Suspended : EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            }).ToList();
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldCalculateCorrectMetadata()
        {
            // Arrange
            var employees = GenerateEmployees(25);
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var query = new GetEmployeesQuery
            {
                PageNumber = 2,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().HaveCount(10);
            result.Pagination.Should().NotBeNull();
            result.Pagination!.TotalCount.Should().Be(25);
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(10);
            result.Pagination.TotalPages.Should().Be(3);
            result.Pagination.HasPreviousPage.Should().BeTrue();
            result.Pagination.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithDepartmentFilter_ShouldReturnOnlyMatchingDepartment()
        {
            // Arrange
            var employees = GenerateEmployees(10);
            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var query = new GetEmployeesQuery
            {
                Department = "Engineering",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().OnlyContain(e => e.Department == "Engineering");
        }

        [Fact]
        public async Task Handle_WithSearchTerm_ShouldFilterMatchingEmployees()
        {
            // Arrange
            var employees = new List<Domain.Entities.Employee>
            {
                new Domain.Entities.Employee { Id = Guid.NewGuid(), FirstName = "Grace", LastName = "Hopper", Email = "grace@navy.mil", Department = "Tech", CreatedAt = DateTime.UtcNow },
                new Domain.Entities.Employee { Id = Guid.NewGuid(), FirstName = "Alan", LastName = "Turing", Email = "alan@enigma.org", Department = "Math", CreatedAt = DateTime.UtcNow },
                new Domain.Entities.Employee { Id = Guid.NewGuid(), FirstName = "Margaret", LastName = "Hamilton", Email = "margaret@nasa.gov", Department = "Tech", CreatedAt = DateTime.UtcNow }
            };

            var mockDbSet = employees.BuildMockDbSet();
            _mockContext.Setup(c => c.Employees).Returns(mockDbSet.Object);

            var query = new GetEmployeesQuery
            {
                Search = "Grace",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Error.Should().BeNull();
            result.Data.Should().HaveCount(1);
            result.Data![0].FirstName.Should().Be("Grace");
        }
    }
}
