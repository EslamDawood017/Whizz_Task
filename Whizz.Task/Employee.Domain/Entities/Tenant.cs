
using Employee.Domain.Common;

namespace Employee.Domain.Entities
{
    public class Tenant : BaseEntity, IAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
