using AutoMapper;
using MiniHR.Application.POCOs.DTOs;
using MiniHR.Application.POCOs.VOs;
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

            // User -> UserVO
            CreateMap<User, UserVO>();
        }
    }
}
