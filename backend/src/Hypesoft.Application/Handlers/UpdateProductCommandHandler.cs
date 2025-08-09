using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Hypesoft.Application.Handlers;

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
            // Manual validation (in addition to FluentValidation if used)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults
                    .Select(r => new ValidationError(r.MemberNames.FirstOrDefault() ?? "Unknown", r.ErrorMessage ?? "Validation error"))
                    .ToList();
                
                _logger.LogWarning("Validation failed for UpdateProductCommand: {Errors}", 
                    JsonSerializer.Serialize(errors));
                    
                return Result.Invalid(errors);
            }

            // Get the existing product
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
                return Result.NotFound($"Product with ID {request.Id} not found");
            }

            // Check if SKU is being changed and if the new SKU is unique
            if (!string.IsNullOrEmpty(request.Sku) && 
                !string.Equals(product.Sku, request.Sku, StringComparison.OrdinalIgnoreCase) &&
                await _uow.Products.IsSkuUniqueAsync(request.Sku, cancellationToken) == false)
            {
                return Result.Error("SKU is already in use by another product");
            }

            // Update product properties
            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.CategoryId,
                request.ImageUrl,
                request.StockQuantity,
                request.DiscountPrice,
                request.IsFeatured,
                request.UserId);

            // Save changes - Usando o método Update da interface IRepository
            _uow.Products.Update(product);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated product with ID {ProductId}", product.Id);

            // Map to DTO and return
            var resultDto = _mapper.Map<ProductDto>(product);
            return Result.Success(resultDto);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error updating product: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", request.Id);
            return Result.Error("An error occurred while updating the product");
        }
    }
}
