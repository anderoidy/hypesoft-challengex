using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly RepositoryBase<Product> _productRepository;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            RepositoryBase<Product> productRepository,
            ILogger<DeleteProductCommandHandler> logger
        )
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to delete product with ID: {ProductId}",
                    request.Id
                );

                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
                    return Result<bool>.NotFound($"Product with ID {request.Id} not found.");
                }

                await _productRepository.DeleteAsync(product, cancellationToken);

                _logger.LogInformation(
                    "Successfully deleted product with ID: {ProductId}",
                    request.Id
                );
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", request.Id);
                return Result<bool>.Error($"Error deleting product: {ex.Message}");
            }
        }
    }
}
