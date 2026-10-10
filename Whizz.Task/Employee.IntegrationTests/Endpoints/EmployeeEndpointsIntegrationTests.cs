using Employee.Application.Common;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using Employee.Infrastructure.Persistence.Data;
using Employee.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Employee.IntegrationTests.Endpoints
{
    public class EmployeeEndpointsIntegrationTests : IntegrationTestBase
    {
        public EmployeeEndpointsIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateEmployee_WithValidPayload_ShouldReturn201Created()
        {
            // Arrange: Seeded Tenant A from DatabaseSeeder
            var client = CreateClientWithTenant(DatabaseSeeder.TenantAId);
            var uniqueEmail = $"john.{Guid.NewGuid():N}@tenant-a.com";

            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = uniqueEmail,
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                Salary = new Money(8500000, "USD"),
                CustomData = "{\"skills\":[\"C#\",\".NET\"]}"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/v1/employees", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
            body.Should().NotBeNull();
            body!.Data.Should().NotBeNull();
            body.Data!.Email.Should().Be(uniqueEmail.ToLowerInvariant());
            body.Data.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task TenantIsolation_EmployeeCreatedInTenantA_ShouldNotBeVisibleInTenantB()
        {
            // Arrange
            var clientA = CreateClientWithTenant(DatabaseSeeder.TenantAId);
            var clientB = CreateClientWithTenant(DatabaseSeeder.TenantBId);

            var command = new CreateEmployeeCommand
            {
                FirstName = "Isolated",
                LastName = "Employee",
                Email = $"isolated.{Guid.NewGuid():N}@tenant-a.com",
                Department = "Security",
                Status = EmployeeStatus.Active
            };

            // Act 1: Create under Tenant A
            var createResponse = await clientA.PostAsJsonAsync("/api/v1/employees", command);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
            var employeeId = created!.Data!.Id;

            // Act 2: Fetch using Tenant A -> Must Succeed (200 OK)
            var getA = await clientA.GetAsync($"/api/v1/employees/{employeeId}");
            getA.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 3: Fetch using Tenant B -> Must return 404 Not Found (Tenant isolation proof)
            var getB = await clientB.GetAsync($"/api/v1/employees/{employeeId}");
            getB.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SoftDeleteEmployee_ShouldPreventSubsequentRetrieval()
        {
            // Arrange
            var client = CreateClientWithTenant(DatabaseSeeder.TenantAId);
            var command = new CreateEmployeeCommand
            {
                FirstName = "To",
                LastName = "Delete",
                Email = $"delete.{Guid.NewGuid():N}@tenant-a.com",
                Department = "Operations",
                Status = EmployeeStatus.Active
            };

            var createResponse = await client.PostAsJsonAsync("/api/v1/employees", command);
            var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
            var employeeId = created!.Data!.Id;

            // Act 1: Soft Delete
            var deleteResponse = await client.DeleteAsync($"/api/v1/employees/{employeeId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 2: Attempt to retrieve -> Global Query Filter should hide soft-deleted entity
            var getResponse = await client.GetAsync($"/api/v1/employees/{employeeId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RowLevelSecurity_WhenBypassingGlobalFilters_DatabaseEngineStillEnforcesTenantBoundary()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantContext = scope.ServiceProvider.GetRequiredService<Employee.Application.Common.Interfaces.ITenantContext>();

            // Act 1: Set Tenant A in tenant context
            tenantContext.SetTenantId(DatabaseSeeder.TenantAId);

            // IgnoreQueryFilters bypasses EF Core application-level filter,
            // but PostgreSQL RLS enforces tenant isolation at the database engine level!
            var employeesTenantA = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(context.Employees.IgnoreQueryFilters());

            // Assert 1: Only Tenant A records are returned from the database
            employeesTenantA.Should().NotBeEmpty();
            employeesTenantA.Should().OnlyContain(e => e.TenantId == DatabaseSeeder.TenantAId);

            // Act 2: Set Tenant B in tenant context
            tenantContext.SetTenantId(DatabaseSeeder.TenantBId);
            var employeesTenantB = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(context.Employees.IgnoreQueryFilters());

            // Assert 2: Only Tenant B records are returned from the database
            employeesTenantB.Should().NotBeEmpty();
            employeesTenantB.Should().OnlyContain(e => e.TenantId == DatabaseSeeder.TenantBId);
        }
    }
}
