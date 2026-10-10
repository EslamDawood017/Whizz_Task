using Employee.Application.Common.Interfaces;
using Employee.Domain.Common;
using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Employee.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ITenantContext _tenantContext;
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Domain.Entities.Employee> Employees { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantContext tenantContext) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableAndGenerateAuditLogs();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditableAndGenerateAuditLogs()
        {
            var utcNow = DateTime.UtcNow;
            var currentTenantId = _tenantContext.TenantId;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            var newAuditLogs = new List<AuditLog>();

            foreach (var entry in entries)
            {
              
                if (entry.Entity is IAuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAt = utcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.UpdatedAt = utcNow;
                    }
                }

               
                if (currentTenantId.HasValue && entry.Entity is BaseEntity baseEntity)
                {
                    var entityName = entry.Entity.GetType().Name;

                    string action = entry.State switch
                    {
                        EntityState.Added => "Created",
                        EntityState.Deleted => "HardDeleted",
                        EntityState.Modified when entry.Properties.Any(p => p.Metadata.Name == "DeletedAt" && p.IsModified && p.CurrentValue != null)
                            => "SoftDeleted",
                        _ => "Updated"
                    };

                    var changes = new Dictionary<string, object?>();

                    if (entry.State == EntityState.Added)
                    {
                        foreach (var prop in entry.Properties.Where(p => !p.Metadata.IsPrimaryKey()))
                        {
                            changes[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        foreach (var prop in entry.Properties.Where(p => p.IsModified && p.Metadata.Name != "UpdatedAt"))
                        {
                            changes[prop.Metadata.Name] = new
                            {
                                Old = prop.OriginalValue,
                                New = prop.CurrentValue
                            };
                        }
                    }

                    var auditLog = new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = currentTenantId.Value,
                        EntityName = entityName,
                        EntityId = baseEntity.Id,
                        Action = action,
                        Timestamp = utcNow,
                        Changes = JsonSerializer.Serialize(changes)
                    };

                    newAuditLogs.Add(auditLog);
                }
            }

            if (newAuditLogs.Any())
            {
                AuditLogs.AddRange(newAuditLogs);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Filter to check tenant with each query
            modelBuilder.Entity<Domain.Entities.Employee>().HasQueryFilter(e =>
                e.TenantId == _tenantContext.TenantId &&
                e.DeletedAt == null);

            modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

            modelBuilder.Entity<CustomFieldDefinition>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        }
    }
}
