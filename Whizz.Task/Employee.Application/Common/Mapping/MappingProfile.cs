using AutoMapper;
using Employee.Application.Features.Employee.Commands;
using Employee.Application.Features.Employee.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Common.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<Domain.Entities.Employee, EmployeeDto>();


       
            CreateMap<CreateEmployeeCommand, Domain.Entities.Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore());
        }
    }
}
