using Microsoft.AspNetCore.Mvc;
using MiniHR.Application.DTOs;
using MiniHR.Application.Interfaces;

namespace MiniHR.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET /api/employees/{id}
        [HttpGet("{id:guid}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        // GET /api/employees
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // POST /api/employees
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            var newEmployee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);

            return CreatedAtAction(
                actionName: nameof(GetByIdAsync),
                routeValues: new { id = newEmployee.Id },
                value: newEmployee
            );
        }
    }
}
