using Employee.Domain.Common;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Domain.Entities
{
    public class Employee : BaseEntity, IMustHaveTenant, ISoftDeletable, IAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public Money? Salary { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public DateTime CustomData { get; set; }
        public Guid TenantId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}

