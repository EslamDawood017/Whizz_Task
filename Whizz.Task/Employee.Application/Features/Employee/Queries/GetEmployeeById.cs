using Employee.Application.Common;
using Employee.Application.Features.Employee.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Queries
{
    public record GetEmployeeByIdQuery(Guid Id) : IRequest<ApiResponse<EmployeeDto>>;
}
