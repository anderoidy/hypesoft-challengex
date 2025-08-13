using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IApplicationUnitOfWork uow,
        IMapper mapper,
        ILogger<UpdateProductCommandHandler> logger
    )
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
            // Manual validation (in addition to FluentValidation if used)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults
                    .Select(r => new ValidationError(
                        r.MemberNames.FirstOrDefault() ?? "Unknown",
                        r.ErrorMessage ?? "Validation error"
                    ))
                    .ToList();

                _logger.LogWarning(
                    "Validation failed for UpdateProductCommand: {Errors}",
                    JsonSerializer.Serialize(errors)
                );

                return Result.Invalid(errors);
            }

            // Get existing product
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
                return Result.NotFound($"Product with ID {request.Id} not found");
            }

            // Check if category exists if it's being updated
            if (request.CategoryId != product.CategoryId)
            {
                var categoryExists = await _uow.Categories.ExistsAsync(
                    c => c.Id == request.CategoryId,
                    cancellationToken
                );
                if (!categoryExists)
                {
                    _logger.LogWarning(
                        "Category with ID {CategoryId} not found",
                        request.CategoryId
                    );
                    return Result.NotFound($"Category with ID {request.CategoryId} not found");
                }
            }

            // Update product properties using the entity's update method
            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.CategoryId,
                request.ImageUrl,
                request.StockQuantity,
                request.DiscountPrice,
                request.IsFeatured,
                request.UserId
            );

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            // Map to DTO using AutoMapper with the CategoryName from context
            var category = await _uow.Categories.GetByIdAsync(
                product.CategoryId,
                cancellationToken
            );
            var productDto = _mapper.Map<ProductDto>(
                product,
                opts => opts.Items["CategoryName"] = category?.Name
            );

            _logger.LogInformation("Updated product with ID {ProductId}", product.Id);
            return Result.Success(productDto);
        }
        catch (DBConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error updating product: {Message}", ex.Message);
            return Result.Error(
                "A concurrency error occurred while updating the product. Please refresh and try again."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Message}", ex.Message);
            return Result.Error($"An error occurred while updating the product: {ex.Message}");
        }
    }
}
