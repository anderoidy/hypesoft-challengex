using Ardalis.Result;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
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

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Usando Remove e SaveChangesAsync em vez de DeleteAsync
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", request.Id);
                return Result.NotFound($"Product with ID {request.Id} not found");
            }

            _uow.Products.Remove(product);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} was deleted", request.Id);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", request.Id);
            return Result.Error($"An error occurred while deleting product with ID {request.Id}");
        }
    }
}