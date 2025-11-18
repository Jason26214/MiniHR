using Microsoft.AspNetCore.Mvc;
using MiniHR.Application.DTOs;
using MiniHR.Application.Interfaces;
using MiniHR.WebAPI.Models;

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

        // GET /api/employees
        [HttpGet]
        public async Task<ApiResult<IEnumerable<EmployeeDto>>> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();

            return new ApiResult<IEnumerable<EmployeeDto>>
            {
                Success = true,
                Code = 200,
                Data = employees,
                Message = "Success"
            };
        }

        // GET /api/employees/{id}
        [HttpGet("{id:guid}")]
        public async Task<ApiResult<EmployeeDto>> GetByIdAsync(Guid id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return new ApiResult<EmployeeDto>
                {
                    Success = false,
                    Code = 404,
                    Error = "Employee not found"
                };
            }

            return new ApiResult<EmployeeDto>
            {
                Success = true,
                Code = 200,
                Data = employee
            };
        }

        // POST /api/employees
        [HttpPost]
        public async Task<ApiResult<EmployeeDto>> CreateAsync([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new ApiResult<EmployeeDto>
                {
                    Success = false,
                    Code = 400,
                    Error = errors,
                    Message = "Validation failed"
                };
            }

            var newEmployee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);

            return new ApiResult<EmployeeDto>
            {
                Success = true,
                Code = 201,
                Data = newEmployee,
                Message = "Employee created successfully"
            };
        }
    }
}