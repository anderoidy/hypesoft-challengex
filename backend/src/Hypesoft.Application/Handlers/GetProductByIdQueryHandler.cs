using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Hypesoft.Application.Queries;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Handlers;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IApplicationUnitOfWork uow, 
        IMapper mapper, 
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Buscando produto com ID {ProductId}", request.Id);
            
            var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", request.Id);
                return Result.NotFound($"Produto com ID {request.Id} não encontrado.");
            }

            // Busca a categoria associada ao produto
            var category = await _uow.Categories.GetByIdAsync(product.CategoryId, cancellationToken);
            
            // Mapeia o produto para DTO e inclui o nome da categoria
            var productDto = _mapper.Map<ProductDto>(product, opts => 
                opts.Items["CategoryName"] = category?.Name);

            _logger.LogInformation("Produto com ID {ProductId} encontrado com sucesso", request.Id);
            return Result.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto com ID {ProductId}", request.Id);
            return Result.Error($"Ocorreu um erro ao buscar o produto: {ex.Message}");
        }
    }
}