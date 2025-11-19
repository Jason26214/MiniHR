using MiniHR.Application.POJOs.DTOs;
using MiniHR.Application.POJOs.VOs;

namespace MiniHR.Application.Interfaces
{
    public interface IEmployeeService
    {
        // GET /api/employees/{id}
        Task<EmployeeVO?> GetByIdAsync(Guid id);

        // GET /api/employees
        Task<IEnumerable<EmployeeVO>> GetAllAsync();

        // POST /api/employees
        Task<EmployeeVO> CreateEmployeeAsync(EmployeeDTO createEmployeeDto);

    }
}
