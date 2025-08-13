using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Application.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => 
                opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));
    }
}
