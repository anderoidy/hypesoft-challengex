using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly RepositoryBase<Product> _productRepository;
        private readonly RepositoryBase<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            RepositoryBase<Product> productRepository,
            RepositoryBase<Category> categoryRepository,
            IMapper mapper,
            ILogger<CreateProductCommandHandler> logger
        )
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Creating new product: {ProductName}", request.Name);

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
                    return Result<Guid>.NotFound(
                        $"Category with ID {request.CategoryId} not found"
                    );
                }

                // Map command to entity
                var product = _mapper.Map<Product>(request);

                // Set audit fields
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                // Add to repository
                await _productRepository.AddAsync(product, cancellationToken);

                _logger.LogInformation(
                    "Successfully created product with ID {ProductId}",
                    product.Id
                );
                return Result<Guid>.Success(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ErrorMessage}", ex.Message);
                return Result<Guid>.Error(ex.Message);
            }
        }
    }
}
