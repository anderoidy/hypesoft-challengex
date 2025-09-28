using AutoMapper;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Product -> ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null)
            );

        // Create -> Product (ignorar campos sensíveis/gerados)
        CreateMap<CreateProductCommand, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore()); // evitar navegação transient

        // Update -> Product (também ignorar para não sobrescrever)
        CreateMap<UpdateProductCommand, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore());
    }
}
