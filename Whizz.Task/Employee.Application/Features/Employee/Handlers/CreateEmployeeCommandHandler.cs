using AutoMapper;
using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using MediatR;
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
        public CreateEmployeeCommandHandler(
            IApplicationDbContext context,
            ITenantContext tenantContext,
            IMapper mapper)
        {
            _context = context;
            _tenantContext = tenantContext;
            _mapper = mapper;
        }
        public async Task<ApiResponse<EmployeeDto>> Handle(
            CreateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            
            var employee = _mapper.Map<Domain.Entities.Employee>(request);

            
            employee.Id = Guid.NewGuid();

            employee.TenantId = _tenantContext.TenantId!.Value; 
            
            employee.CreatedAt = DateTime.UtcNow;

            employee.CustomData = string.IsNullOrWhiteSpace(request.CustomData) ? "{}" : request.CustomData;

            await _context.Employees.AddAsync(employee, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return ApiResponse<EmployeeDto>.Success(employeeDto);
        }
    }
}
