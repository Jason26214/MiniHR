using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniHR.Application.Interfaces;
using MiniHR.Application.POCOs.DTOs;
using MiniHR.Application.POCOs.VOs;

namespace MiniHR.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // jwt token is required
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET /api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeVO>>> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();

            return Ok(employees);
        }

        // GET /api/employees/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeVO>> GetByIdAsync(Guid id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: $"Employee with ID {id} was not found."
                    );
            }

            return Ok(employee);
        }

        // POST /api/employees
        [HttpPost]
        public async Task<ActionResult<EmployeeVO>> CreateAsync([FromBody] EmployeeDTO createEmployeeDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            EmployeeVO newEmployee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);

            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { id = newEmployee.Id },
                newEmployee
            );
        }
    }
}