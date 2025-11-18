using MiniHR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        // Get an employee by id
        Task<Employee?> GetByIdAsync(Guid id);

        // Get all employees
        Task<IEnumerable<Employee>> GetAllAsync();

        // Get an employee by email
        Task<Employee?> GetByEmailAsync(string email);

        // Add a new employee
        Task AddAsync(Employee employee);
    }
}
