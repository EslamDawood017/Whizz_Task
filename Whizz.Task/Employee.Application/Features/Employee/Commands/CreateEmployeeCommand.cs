using Employee.Application.Common;
using Employee.Application.Features.Employee.DTOs;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Commands
{
    public class CreateEmployeeCommand : IRequest<ApiResponse<EmployeeDto>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public string? CustomData { get; set; }
        public Money? Salary { get; set; }
    }
}
