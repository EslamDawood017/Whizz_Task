using Employee.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Commands
{
    public record DeleteEmployeeCommand(Guid Id) : IRequest<ApiResponse<bool>>;
}
