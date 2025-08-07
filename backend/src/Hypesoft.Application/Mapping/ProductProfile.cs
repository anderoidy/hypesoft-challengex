using AutoMapper;
using Hypesoft.Domain.Entities;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Commands;

namespace Hypesoft.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}