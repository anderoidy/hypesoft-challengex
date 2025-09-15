using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetAllProductsQueryHandler
        : IRequestHandler<GetAllProductsQuery, Result<PaginatedList<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Result<PaginatedList<ProductDto>>> Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // ✅ Use os métodos da interface IProductRepository
                var products = await _productRepository.GetAllAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    search: request.SearchTerm
                );

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

                var paginatedList = new PaginatedList<ProductDto>(
                    productDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result.Success(paginatedList);
            }
            catch (Exception ex)
            {
                return Result.Error($"Erro ao buscar produtos: {ex.Message}");
            }
        }
    }
}
