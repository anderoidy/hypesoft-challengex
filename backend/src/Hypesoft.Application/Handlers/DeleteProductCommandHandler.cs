using Ardalis.Result;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IApplicationUnitOfWork uow,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", request.Id);
                return Result.NotFound($"Product with ID {request.Id} not found");
            }

            // Check if product can be deleted (e.g., no associated orders)
            // var isProductInUse = await _uow.Products.IsProductInUseAsync(request.Id, cancellationToken);
            // if (isProductInUse)
            // {
            //     return Result.Error("Cannot delete product as it is associated with existing orders");
            // }

            await _uow.Products.DeleteAsync(product, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} was deleted", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", request.Id);
            return Result.Error($"An error occurred while deleting product with ID {request.Id}");
        }
    }
}
