using Employee.Application.Common.Interfaces;
using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Domain.Entities.Employee> Employees { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ITenantContext tenantContext ) : base(options)
        {
            _tenantContext = tenantContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            //filter to check tenant with each query
            modelBuilder.Entity<Domain.Entities.Employee>().HasQueryFilter(e =>
                e.TenantId == _tenantContext.TenantId &&
                e.DeletedAt == null);

            modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId );

            modelBuilder.Entity<CustomFieldDefinition>().HasQueryFilter(e =>  e.TenantId == _tenantContext.TenantId );
        }
    }
}
