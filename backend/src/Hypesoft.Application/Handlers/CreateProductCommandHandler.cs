using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Handlers;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IApplicationUnitOfWork uow, IMapper mapper, ILogger<CreateProductCommandHandler> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validação de unicidade de SKU e Barcode
            if (!string.IsNullOrWhiteSpace(request.Sku) && !await _uow.Products.IsSkuUniqueAsync(request.Sku, cancellationToken))
                return Result.Invalid(new() { { nameof(request.Sku), "SKU já existe." } });

            if (!string.IsNullOrWhiteSpace(request.Barcode) && !await _uow.Products.IsBarcodeUniqueAsync(request.Barcode, cancellationToken))
                return Result.Invalid(new() { { nameof(request.Barcode), "Código de barras já existe." } });

            // Validação de categoria
            var category = await _uow.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                return Result.Invalid(new() { { nameof(request.CategoryId), "Categoria não encontrada." } });

            // Criação do produto
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
                request.IsPublished,
                request.UserId);

            await _uow.Products.AddAsync(product, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Produto criado com ID {ProductId}", product.Id);
            return Result.Success(product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            return Result.Error("Erro inesperado ao criar produto.");
        }
    }
}