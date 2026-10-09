using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<Domain.Entities.Employee> Employees { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
