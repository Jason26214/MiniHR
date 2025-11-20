using AutoMapper;
using MiniHR.Application.Interfaces;
using MiniHR.Application.POCOs.DTOs;
using MiniHR.Application.POCOs.VOs;
using MiniHR.Domain.Entities;
using MiniHR.Domain.Exceptions;
using MiniHR.Domain.Interfaces;

namespace MiniHR.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        // GET /api/employees/{id}
        public async Task<EmployeeVO?> GetByIdAsync(Guid id)
        {
            Employee? employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return null;
            }
            return _mapper.Map<EmployeeVO>(employee);
        }

        // GET /api/employees
        public async Task<IEnumerable<EmployeeVO>> GetAllAsync()
        {
            IEnumerable<Employee> employees = await _employeeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmployeeVO>>(employees);
        }

        // POST /api/employees
        public async Task<EmployeeVO> CreateEmployeeAsync(EmployeeDTO createEmployeeDto)
        {
            var existing = await _employeeRepository.GetByEmailAsync(createEmployeeDto.Email);
            if (existing != null)
            {
                throw new DuplicateEmailException(createEmployeeDto.Email);
            }

            Employee employeeEntity = _mapper.Map<Employee>(createEmployeeDto);
            await _employeeRepository.AddAsync(employeeEntity);
            await _employeeRepository.SaveChangesAsync();
            return _mapper.Map<EmployeeVO>(employeeEntity);
        }
    }


}
