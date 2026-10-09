using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Domain.Common
{
    public interface ISoftDeletable
    {
        public DateTime? DeletedAt { get; set; }
    }
}
