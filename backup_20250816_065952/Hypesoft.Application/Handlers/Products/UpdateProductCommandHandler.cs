using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IApplicationUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IApplicationUnitOfWork uow,
            IMapper mapper,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating product with ID {ProductId}", request.Id);

                // Get existing product
                var existingProduct = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for update", request.Id);
                    return Result.NotFound($"Product with ID {request.Id} not found");
                }

                // Check if category exists
                var categoryExists = await _uow.Categories.ExistsAsync(
                    c => c.Id == request.CategoryId, 
                    cancellationToken);
                if (!categoryExists)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", request.CategoryId);
                    return Result.NotFound($"Category with ID {request.CategoryId} not found");
                }

                // Check SKU uniqueness if provided
                if (!string.IsNullOrEmpty(request.Sku) && 
                    !await _uow.Products.IsSkuUniqueAsync(request.Sku, request.Id, cancellationToken))
                {
                    _logger.LogWarning("SKU {Sku} is already in use", request.Sku);
                    return Result.Conflict($"SKU '{request.Sku}' is already in use by another product");
                }

                // Map updates to existing product
                _mapper.Map(request, existingProduct);
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Update product
                _uow.Products.Update(existingProduct);
                await _uow.CommitAsync(cancellationToken);

                // Map to DTO for response
                var updatedProductDto = _mapper.Map<ProductDto>(existingProduct);

                _logger.LogInformation("Successfully updated product with ID {ProductId}", request.Id);
                return Result.Success(updatedProductDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}: {ErrorMessage}", 
                    request.Id, ex.Message);
                return Result.Error($"An error occurred while updating the product: {ex.Message}");
            }
        }
    }
}
