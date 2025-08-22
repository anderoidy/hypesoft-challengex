using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class UpdateProductCommandHandler
        : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly RepositoryBase<Product> _productRepository;
        private readonly RepositoryBase<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            RepositoryBase<Product> productRepository,
            RepositoryBase<Category> categoryRepository,
            IMapper mapper,
            ILogger<UpdateProductCommandHandler> logger
        )
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<ProductDto>> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Updating product with ID {ProductId}", request.Id);

                // Get existing product
                var existingProduct = await _productRepository.GetByIdAsync(
                    request.Id,
                    cancellationToken
                );
                if (existingProduct == null)
                {
                    _logger.LogWarning(
                        "Product with ID {ProductId} not found for update",
                        request.Id
                    );
                    return Result<ProductDto>.NotFound($"Product with ID {request.Id} not found");
                }

                // Check if category exists
                var category = await _categoryRepository.GetByIdAsync(
                    request.CategoryId,
                    cancellationToken
                );
                if (category == null)
                {
                    _logger.LogWarning(
                        "Category with ID {CategoryId} not found",
                        request.CategoryId
                    );
                    return Result<ProductDto>.NotFound(
                        $"Category with ID {request.CategoryId} not found"
                    );
                }

                // Check SKU uniqueness if provided
                if (!string.IsNullOrEmpty(request.Sku))
                {
                    var allProducts = await _productRepository.ListAsync(cancellationToken);
                    var skuExists = allProducts.Any(p =>
                        p.Sku == request.Sku && p.Id != request.Id
                    );

                    if (skuExists)
                    {
                        _logger.LogWarning("SKU {Sku} is already in use", request.Sku);
                        return Result<ProductDto>.Conflict(
                            $"SKU '{request.Sku}' is already in use by another product"
                        );
                    }
                }

                // Map updates to existing product
                _mapper.Map(request, existingProduct);
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Update product
                await _productRepository.UpdateAsync(existingProduct, cancellationToken);

                // Map to DTO for response
                var updatedProductDto = _mapper.Map<ProductDto>(existingProduct);

                _logger.LogInformation(
                    "Successfully updated product with ID {ProductId}",
                    request.Id
                );
                return Result<ProductDto>.Success(updatedProductDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating product with ID {ProductId}: {ErrorMessage}",
                    request.Id,
                    ex.Message
                );
                return Result<ProductDto>.Error(
                    $"An error occurred while updating the product: {ex.Message}"
                );
            }
        }
    }
}
