using Microsoft.EntityFrameworkCore;
using MiniHR.Domain.Entities;
using MiniHR.Domain.Interfaces;
using MiniHR.Infrastructure.Persistence;

namespace MiniHR.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly MiniHrDbContext _context;

        public EmployeeRepository(MiniHrDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await _context.Employees
                            .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted == false);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                            .Where(e => e.IsDeleted == false)
                            .ToListAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                            .FirstOrDefaultAsync(e => e.Email == email && e.IsDeleted == false);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }


    }
}
