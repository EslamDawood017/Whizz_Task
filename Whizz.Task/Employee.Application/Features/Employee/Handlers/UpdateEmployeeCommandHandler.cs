using AutoMapper;
using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Employee.Application.Features.Employee.Handlers;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICustomDataValidator _customDataValidator;

    public UpdateEmployeeCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICustomDataValidator customDataValidator)
    {
        _context = context;
        _mapper = mapper;
        _customDataValidator = customDataValidator;
    }

    public async Task<ApiResponse<EmployeeDto>> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
      
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
        {
            return ApiResponse<EmployeeDto>.Failure("Employee not found.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailTaken = await _context.Employees
            .AnyAsync(e => e.Id != request.Id && e.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            return ApiResponse<EmployeeDto>.Failure($"An employee with email '{request.Email}' already exists.");
        }

        var (isValid, validationError) = await _customDataValidator.ValidateAsync(request.CustomData, cancellationToken);
        if (!isValid)
        {
            return ApiResponse<EmployeeDto>.Failure(validationError!);
        }

       
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = normalizedEmail;
        employee.Department = request.Department;
        employee.Status = request.Status;
        employee.Salary = request.Salary;
        employee.CustomData = string.IsNullOrWhiteSpace(request.CustomData) ? "{}" : request.CustomData;
        employee.UpdatedAt = DateTime.UtcNow;

        
        await _context.SaveChangesAsync(cancellationToken);

        
        var dto = _mapper.Map<EmployeeDto>(employee);
        return ApiResponse<EmployeeDto>.Success(dto);
    }
}
