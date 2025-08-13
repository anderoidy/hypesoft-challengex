using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Hypesoft.Application.Common;
using Hypesoft.Application.Common.Models;

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
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Build the predicate for filtering
            System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null;
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                predicate = p => p.Name.ToLower().Contains(searchTerm) || 
                               (p.Description != null && p.Description.ToLower().Contains(searchTerm));
            }

            // Get paginated results
            var (items, totalCount) = await _uow.Products.GetPagedAsync(
                predicate: predicate,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken);

            // Map to DTOs
            var itemsDto = _mapper.Map<List<ProductDto>>(items);
            
            // Create paginated result
            var paginatedResult = new PaginatedList<ProductDto>(
                itemsDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} of {TotalCount} products", 
                itemsDto.Count, totalCount);
                
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return Result.Error($"An error occurred while retrieving products: {ex.Message}");
        }
    }
}
