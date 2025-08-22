using System;
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
                _logger.LogInformation("Creating new product: {ProductName}", request.Name);

                // Check if category exists
                var category = await _uow.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", request.CategoryId);
                    return Result.NotFound($"Category with ID {request.CategoryId} not found");
                }

                // Map command to entity
                var product = _mapper.Map<Product>(request);

                // Set audit fields
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                // Add to repository
                await _uow.Products.AddAsync(product, cancellationToken);
                await _uow.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully created product with ID {ProductId}", product.Id);
                return Result.Success(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ErrorMessage}", ex.Message);
                return Result.Error(ex.Message);
            }
        }
    }
}
