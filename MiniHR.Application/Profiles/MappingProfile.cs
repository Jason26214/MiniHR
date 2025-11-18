using AutoMapper;
using MiniHR.Application.DTOs;
using MiniHR.Domain.Entities;

namespace MiniHR.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Read data
            CreateMap<Employee, EmployeeDto>();
            // Create data
            CreateMap<CreateEmployeeDto, Employee>();
        }
    }
}
