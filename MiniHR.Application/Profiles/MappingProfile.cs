using AutoMapper;
using MiniHR.Application.POJOs.DTOs;
using MiniHR.Application.POJOs.VOs;
using MiniHR.Domain.Entities;

namespace MiniHR.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Read data -> VO
            CreateMap<Employee, EmployeeVO>();
            // Create data -> Entity
            CreateMap<EmployeeDTO, Employee>();
        }
    }
}
