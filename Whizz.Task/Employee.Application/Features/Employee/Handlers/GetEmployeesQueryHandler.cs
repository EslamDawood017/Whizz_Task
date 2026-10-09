using AutoMapper;
using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.DTOs;
using Employee.Application.Features.Employee.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Employee.Application.Features.Employee.Handlers;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, ApiResponse<List<EmployeeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEmployeesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<EmployeeDto>>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Employees.AsNoTracking();


        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(e => e.Department.ToLower() == request.Department.Trim().ToLower());
        }

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(searchTerm) ||
                e.LastName.ToLower().Contains(searchTerm) ||
                e.Email.ToLower().Contains(searchTerm));
        }

       
        var totalCount = await query.CountAsync(cancellationToken);

       
        var employees = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

       
        var dtos = _mapper.Map<List<EmployeeDto>>(employees);

       
        var pagination = new PaginationMetadata(request.PageNumber, request.PageSize, totalCount);

        return ApiResponse<List<EmployeeDto>>.Success(dtos, pagination);
    }
}
