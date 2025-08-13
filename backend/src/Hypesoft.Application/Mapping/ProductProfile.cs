using AutoMapper;
using Hypesoft.Domain.Entities;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Commands;

namespace Hypesoft.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Mapeamento de Product para ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => 
                opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
                    
        // Mapeamento de comandos para a entidade Product
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}