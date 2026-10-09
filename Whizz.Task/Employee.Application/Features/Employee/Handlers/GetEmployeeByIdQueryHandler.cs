using AutoMapper;
using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.DTOs;
using Employee.Application.Features.Employee.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Features.Employee.Handlers
{
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, ApiResponse<EmployeeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetEmployeeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ApiResponse<EmployeeDto>> Handle(
            GetEmployeeByIdQuery request,
            CancellationToken cancellationToken)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (employee == null)
            {
                return ApiResponse<EmployeeDto>.Failure("Employee not found.");
            }

            var dto = _mapper.Map<EmployeeDto>(employee);
            return ApiResponse<EmployeeDto>.Success(dto);
        }
    }
}
