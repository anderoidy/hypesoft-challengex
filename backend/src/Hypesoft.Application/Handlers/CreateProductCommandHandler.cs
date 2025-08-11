using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Hypesoft.Application.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IApplicationUnitOfWork uow,
        IMapper mapper,
        ILogger<CreateProductCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate SKU uniqueness only if SKU is provided
            if (!string.IsNullOrEmpty(request.Sku) && 
                !await _uow.Products.IsSkuUniqueAsync(request.Sku, false, cancellationToken))
            {
                return Result.Invalid(new ValidationError
                {
                    Identifier = nameof(request.Sku),
                    ErrorMessage = "SKU must be unique"
                });
            }

            // Check if category exists
            var categoryExists = await _uow.Categories.ExistsAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", request.CategoryId);
                return Result.NotFound($"Category with ID {request.CategoryId} not found");
            }

            // Create new product using the factory method
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.CategoryId,
                request.Sku,
                request.Barcode,
                request.DiscountPrice,
                request.StockQuantity,
                request.ImageUrl,
                request.Weight,
                request.Height,
                request.Width,
                request.Length,
                request.IsFeatured,
                request.IsPublished);

            // Add product to repository
            await _uow.Products.AddAsync(product, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created product with ID {ProductId}", product.Id);
            return Result.Success(product.Id);
        }
        catch (DBConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error creating product: {Message}", ex.Message);
            return Result.Error("A concurrency error occurred while creating the product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {Message}", ex.Message);
            return Result.Error($"An error occurred while creating the product: {ex.Message}");
        }
    }
}