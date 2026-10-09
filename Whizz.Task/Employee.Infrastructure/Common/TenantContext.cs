using Employee.Application.Common.Interfaces;

namespace Employee.Infrastructure.Common
{
    public class TenantContext : ITenantContext
    {
        public Guid? TenantId { set; get; }

        public bool HasTenant { set; get; }


        public void SetTenantId(Guid tenantId)
        {
            TenantId = tenantId;
            HasTenant = true;
        }
    }
}
