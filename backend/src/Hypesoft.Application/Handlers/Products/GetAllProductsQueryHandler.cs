using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Specifications;
using MediatR;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetAllProductsQueryHandler
        : IRequestHandler<GetAllProductsQuery, Result<PaginatedList<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Result<PaginatedList<ProductDto>>> Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken
        )
        {
            // Get paginated products with search
            var products = await _productRepository.GetAllAsync(
                request.PageNumber, 
                request.PageSize, 
                request.SearchTerm
            );

            // Get total count for pagination
            var totalCount = await _productRepository.GetTotalCountAsync(request.SearchTerm);

            var productDtos = products
                .Select(p => new ProductDto(
                    Id: p.Id,
                    Name: p.Name,
                    Description: p.Description,
                    ImageUrl: p.ImageUrl,
                    Price: p.Price,
                    DiscountPrice: p.DiscountPrice,
                    StockQuantity: p.StockQuantity,
                    Sku: p.Sku,
                    Barcode: p.Barcode,
                    IsFeatured: p.IsFeatured,
                    IsPublished: p.IsPublished,
                    PublishedAt: p.PublishedAt,
                    CategoryId: p.CategoryId,
                    CategoryName: p.Category?.Name
                ))
                .ToList();

            return Result.Success(
                new PaginatedList<ProductDto>(
                    productDtos, 
                    totalCount, 
                    request.PageNumber, 
                    request.PageSize
                )
            );
        }
    }
}
