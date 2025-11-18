using AutoMapper;
using MiniHR.Application.DTOs;
using MiniHR.Application.Interfaces;
using MiniHR.Domain.Entities;
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
        public async Task<EmployeeDto?> GetByIdAsync(Guid id)
        {
            Employee? employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return null;
            }
            return _mapper.Map<EmployeeDto>(employee);
        }

        // GET /api/employees
        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            IEnumerable<Employee> employees = await _employeeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        // POST /api/employees
        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            Employee employeeEntity = _mapper.Map<Employee>(createEmployeeDto);
            await _employeeRepository.AddAsync(employeeEntity);
            await _employeeRepository.SaveChangesAsync();
            return _mapper.Map<EmployeeDto>(employeeEntity);
        }


    }
}
