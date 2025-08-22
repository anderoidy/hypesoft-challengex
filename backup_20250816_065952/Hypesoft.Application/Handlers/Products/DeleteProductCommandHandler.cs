using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly IApplicationUnitOfWork _uow;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IApplicationUnitOfWork uow,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID {ProductId}", request.Id);

                var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for deletion", request.Id);
                    return Result.NotFound($"Product with ID {request.Id} not found");
                }

                _uow.Products.Remove(product);
                await _uow.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted product with ID {ProductId}", request.Id);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}: {ErrorMessage}", request.Id, ex.Message);
                return Result.Error($"An error occurred while deleting the product: {ex.Message}");
            }
        }
    }
}
