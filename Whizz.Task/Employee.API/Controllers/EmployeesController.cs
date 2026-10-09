using Employee.Application.Common;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers
{
    [Route("api/v1/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ISender _sender;

        public EmployeesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
        {
            var response = await _sender.Send(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
