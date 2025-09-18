using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetProductByIdQueryHandler
        : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        // ✅ CORREÇÃO: Construtor deve receber IProductRepository
        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Result<ProductDto>> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            // ✅ CORREÇÃO: Remover cancellationToken se sua interface não tiver
            var product = await _productRepository.GetByIdAsync(request.Id);

            if (product == null)
                return Result<ProductDto>.NotFound("Product not found");

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

            return Result<ProductDto>.Success(productDto);
        }
    }
}
