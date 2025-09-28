using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Interfaces;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
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
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
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

                // Set audit data through method, no direct property assignment
                product.SetLastModifiedBy(request.CreatedBy ?? "system");

                // Add to repository (passing only product as param)
                await _productRepository.AddAsync(product);

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
