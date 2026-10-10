using AutoMapper;
using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Handlers
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ApiResponse<EmployeeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IMapper _mapper;
        private readonly ICustomDataValidator _customDataValidator;
        public CreateEmployeeCommandHandler(
            IApplicationDbContext context,
            ITenantContext tenantContext,
            IMapper mapper,
            ICustomDataValidator customDataValidator)
        {
            _context = context;
            _tenantContext = tenantContext;
            _mapper = mapper;
        }
        public async Task<ApiResponse<EmployeeDto>> Handle(
            CreateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
             var normalizedEmail = request.Email.Trim().ToLowerInvariant();

             var emailExists = await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == normalizedEmail, cancellationToken);
            
            if (emailExists)
            {
                return ApiResponse<EmployeeDto>.Failure($"An employee with email '{request.Email}' already exists.");
            }

            var (isValid, validationError) = await _customDataValidator.ValidateAsync(request.CustomData, cancellationToken);
            if (!isValid)
            {
                return ApiResponse<EmployeeDto>.Failure(validationError!);
            }

            var employee = _mapper.Map<Domain.Entities.Employee>(request);
            employee.Id = Guid.NewGuid();
            employee.TenantId = _tenantContext.TenantId!.Value;
            employee.Email = normalizedEmail; // Store normalized email
            employee.CreatedAt = DateTime.UtcNow;
            employee.CustomData = string.IsNullOrWhiteSpace(request.CustomData) ? "{}" : request.CustomData;

            await _context.Employees.AddAsync(employee, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return ApiResponse<EmployeeDto>.Success(employeeDto);
        }
    }
}
