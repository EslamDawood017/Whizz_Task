using Employee.Domain.Entities;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;


namespace Employee.Infrastructure.Persistence.Data
{
    public static class DatabaseSeeder
    {
        // Define deterministic, fixed GUIDs
        public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. Seed Tenants
            if (!await context.Tenants.AnyAsync())
            {
                var tenants = new List<Tenant>
            {
                new Tenant
                {
                    Id = TenantAId,
                    Name = "Tenant A Corp",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Tenant
                {
                    Id = TenantBId,
                    Name = "Tenant B Industries",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

                await context.Tenants.AddRangeAsync(tenants);
                await context.SaveChangesAsync();
            }

            if (!await context.Employees.IgnoreQueryFilters().AnyAsync())
            {
                var employees = new List<Domain.Entities.Employee>
            {
                // Employees for Tenant A
                new Domain.Entities.Employee
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    TenantId = TenantAId,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@tenanta.com",
                    Department = "Engineering",
                    Status = EmployeeStatus.Active,
                    CustomData = "{}",
                    Salary = new Money(500000, "USD"), 
                    CreatedAt = DateTime.UtcNow
                },
                new Domain.Entities.Employee
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    TenantId = TenantAId,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@tenanta.com",
                    Department = "HR",
                    Status = EmployeeStatus.Active,
                    CustomData = "{}",
                    Salary = new Money(600000, "USD"),
                    CreatedAt = DateTime.UtcNow
                },

                // Employees for Tenant B
                new Domain.Entities.Employee
                {
                    Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    TenantId = TenantBId,
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@tenantb.com",
                    Department = "Finance",
                    Status = EmployeeStatus.Active,
                    CustomData = "{}",
                    Salary = new Money(550000, "USD"),
                    CreatedAt = DateTime.UtcNow
                },
                new Domain.Entities.Employee
                {
                    Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    TenantId = TenantBId,
                    FirstName = "Alice",
                    LastName = "Williams",
                    Email = "alice.williams@tenantb.com",
                    Department = "Marketing",
                    Status = EmployeeStatus.Active,
                    CustomData = "{}",
                    Salary = new Money(650000, "USD"),
                    CreatedAt = DateTime.UtcNow
                }
            };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();
            }
        }
    }
}
