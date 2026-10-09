using Employee.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Domain.Entities
{
    public class CustomFieldDefinition : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    

        public Tenant? Tenant { get; set; } 
        
    }
}