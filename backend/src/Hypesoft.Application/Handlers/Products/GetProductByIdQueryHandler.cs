using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetProductByIdQueryHandler
        : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly RepositoryBase<Product> _productRepository;

        public GetProductByIdQueryHandler(RepositoryBase<Product> productRepository)
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Result<ProductDto>> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
                return Result.NotFound("Product not found");

            var productDto = new ProductDto(
                Id: product.Id,
                Name: product.Name,
                Description: product.Description,
                ImageUrl: product.ImageUrl,
                Price: product.Price,
                DiscountPrice: product.DiscountPrice,
                StockQuantity: product.StockQuantity,
                Sku: product.Sku,
                Barcode: product.Barcode,
                IsFeatured: product.IsFeatured,
                IsPublished: product.IsPublished,
                PublishedAt: product.PublishedAt,
                CategoryId: product.CategoryId,
                CategoryName: product.Category?.Name
            );

            return Result.Success(productDto);
        }
    }
}
