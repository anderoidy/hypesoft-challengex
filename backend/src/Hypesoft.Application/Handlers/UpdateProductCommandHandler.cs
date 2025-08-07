using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IApplicationUnitOfWork uow,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Busca o produto existente
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update", request.Id);
                return Result.NotFound($"Product with ID {request.Id} not found");
            }

            // Valida a categoria se fornecida
            if (request.CategoryId.HasValue)
            {
                var category = await _uow.Categories.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (category == null)
                {
                    return Result.Invalid(new() { { nameof(request.CategoryId), "Category not found" } });
                }
            }

            // Atualiza as propriedades do produto
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            
            if (request.CategoryId.HasValue)
                product.CategoryId = request.CategoryId.Value;
                
            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                product.ImageUrl = request.ImageUrl;

            // Atualiza as tags se fornecidas
            if (request.TagIds != null && request.TagIds.Count > 0)
            {
                var tags = await _uow.Tags.GetTagsByIdsAsync(request.TagIds, cancellationToken);
                if (tags.Count != request.TagIds.Count)
                {
                    return Result.Invalid(new() { { nameof(request.TagIds), "One or more tags not found" } });
                }
                product.Tags = tags;
            }

            // Atualiza a data de modificação
            product.UpdatedAt = DateTime.UtcNow;

            // Salva as alterações
            _uow.Products.Update(product);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} was successfully updated", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", request.Id);
            return Result.Error($"An error occurred while updating product with ID {request.Id}");
        }
    }
}
