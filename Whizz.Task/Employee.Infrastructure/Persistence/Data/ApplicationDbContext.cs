using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Domain.Entities.Employee> Employees { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
