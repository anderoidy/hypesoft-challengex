using System;
using System.Linq;
using AutoMapper;
using Hypesoft.Application.Common.Dtos;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationRole, RoleDto>()
            .ForMember(dest => dest.Claims, opt => 
                opt.MapFrom(src => src.Claims.Select(c => c.ClaimValue)));
    }
}
