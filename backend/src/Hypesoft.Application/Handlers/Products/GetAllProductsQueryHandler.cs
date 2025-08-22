using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetAllProductsQueryHandler
        : IRequestHandler<GetAllProductsQuery, Result<PaginatedList<ProductDto>>>
    {
        private readonly RepositoryBase<Product> _productRepository;

        public GetAllProductsQueryHandler(RepositoryBase<Product> productRepository)
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Result<PaginatedList<ProductDto>>> Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken
        )
        {
            // ✅ Use ListAsync diretamente do RepositoryBase (sem specification por enquanto)
            var products = await _productRepository.ListAsync(cancellationToken);

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
                new PaginatedList<ProductDto>(productDtos, productDtos.Count, 1, 10)
            );
        }
    }
}
