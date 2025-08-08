using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Common;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IApplicationUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(
        IApplicationUnitOfWork uow, 
        IMapper mapper, 
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Buscando produtos com os parâmetros: SearchTerm={SearchTerm}, PageNumber={PageNumber}, PageSize={PageSize}", 
                request.SearchTerm, request.PageNumber, request.PageSize);

            // Prepara a expressão de filtro
            Expression<Func<Product, bool>>? filter = null;
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                filter = p => p.Name.ToLower().Contains(searchTerm) || 
                             (p.Description != null && p.Description.ToLower().Contains(searchTerm));
            }

            // Obtém os produtos paginados do repositório
            var (products, totalCount) = await _uow.Products.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                predicate: filter,
                orderBy: q => q.OrderBy(p => p.Name),
                includeProperties: "Category",
                cancellationToken: cancellationToken);

            // Mapeia os produtos para DTOs
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            // Preenche o nome da categoria em cada DTO
            foreach (var product in products)
            {
                var productDto = productDtos.FirstOrDefault(p => p.Id == product.Id);
                if (productDto != null && product.Category != null)
                {
                    productDto.CategoryName = product.Category.Name;
                }
            }

            // Cria o resultado paginado
            var paginatedResult = new PaginatedList<ProductDto>(
                items: productDtos,
                count: totalCount,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            _logger.LogInformation("Encontrados {Count} produtos (página {PageNumber} de {TotalPages})", 
                productDtos.Count, request.PageNumber, paginatedResult.TotalPages);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos");
            return Result.Error("Ocorreu um erro ao buscar os produtos. Por favor, tente novamente mais tarde.");
        }
    }
}
