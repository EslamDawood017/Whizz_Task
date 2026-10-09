using Employee.Application.Common;
using Employee.Application.Features.Employee.DTOs;
using Employee.Domain.Enums;
using Employee.Domain.ValueObjects;
using MediatR;
using System.Text.Json.Serialization;

namespace Employee.Application.Features.Employee.Commands;

public class UpdateEmployeeCommand : IRequest<ApiResponse<EmployeeDto>>
{
    
    [JsonIgnore]
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; }
    public string? CustomData { get; set; }
    public Money? Salary { get; set; }
}
