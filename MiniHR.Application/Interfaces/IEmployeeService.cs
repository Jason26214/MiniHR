
using MiniHR.Application.DTOs;

namespace MiniHR.Application.Interfaces
{
    public interface IEmployeeService
    {
        // GET /api/employees/{id}
        Task<EmployeeDto?> GetByIdAsync(Guid id);

        // GET /api/employees
        Task<IEnumerable<EmployeeDto>> GetAllAsync();

        // POST /api/employees
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);

    }
}
