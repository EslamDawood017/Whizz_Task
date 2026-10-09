using Employee.Application.Common;
using Employee.Application.Features.Employee.DTOs;
using Employee.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Queries
{
    public class GetEmployeesQuery : IRequest<ApiResponse<List<EmployeeDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Department { get; set; }
        public EmployeeStatus? Status { get; set; }
        public string? Search { get; set; }
    }
}
