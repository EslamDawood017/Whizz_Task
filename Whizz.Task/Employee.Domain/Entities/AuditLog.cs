
using Employee.Domain.Common;

namespace Employee.Domain.Entities
{
    public class AuditLog : BaseEntity , IMustHaveTenant 
    {
        public string EntityName { get; set; } = string.Empty;
        public  Guid EntityId { get; set; }
        public  Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Guid TenantId { get ; set; }
        public string Changes { get; set; } = string.Empty;
    }
}
