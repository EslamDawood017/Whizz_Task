using Employee.Application.Common;
using Employee.Application.Common.Interfaces;
using Employee.Application.Features.Employee.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Employee.Application.Features.Employee.Handlers;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
        {
            return ApiResponse<bool>.Failure("Employee not found.");
        }

        
        employee.DeletedAt = DateTime.UtcNow;

        
        await _context.SaveChangesAsync(cancellationToken);

        
        return ApiResponse<bool>.Success(true);
    }
}
