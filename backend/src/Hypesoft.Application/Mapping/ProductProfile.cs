using AutoMapper;
using Hypesoft.Domain.Entities;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Commands;

namespace Hypesoft.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => 
                opt.MapFrom((src, dest, destMember, context) => 
                    context.Items.ContainsKey("CategoryName") ? context.Items["CategoryName"] as string : null));
                    
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}