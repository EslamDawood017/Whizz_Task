using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Common.Interfaces
{
    public interface ICustomDataValidator
    {
        Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
            string? customDataJson,
            CancellationToken cancellationToken = default);
    }
}
