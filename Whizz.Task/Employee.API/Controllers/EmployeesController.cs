using Employee.Application.Common;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using Employee.Application.Features.Employee.Queries;
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


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        Guid id,
        [FromBody] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
        {
            command.Id = id;
            var response = await _sender.Send(command, cancellationToken);
            if (response.Error != null)
            {
                if (response.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return Conflict(response);
            }
            return Ok(response); 
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAll(
        [FromQuery] GetEmployeesQuery query,
        CancellationToken cancellationToken)
        {
            var response = await _sender.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);

            if (response.Error != null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
        {
            var response = await _sender.Send(command, cancellationToken);

            if (response.Error != null)
            {
                return Conflict(response);
            }
            return StatusCode(StatusCodes.Status201Created, response);
        }
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
            if (response.Error != null)
            {
                return NotFound(response); 
            }
            return Ok(response); 
        }
    }
}
