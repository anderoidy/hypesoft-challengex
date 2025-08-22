using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.Common.Models;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
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
                _logger.LogInformation("Fetching products with page {PageNumber} and page size {PageSize}", 
                    request.PageNumber, request.PageSize);

                // Create specification with search term and pagination
                var spec = new ProductsWithCategorySpecification(
                    searchTerm: request.SearchTerm,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize);

                // Get paginated result using the specification
                var products = await _uow.Products.ListAsync(spec, cancellationToken);
                var totalCount = await _uow.Products.CountAsync(spec.Criteria, cancellationToken);

                // Map to DTOs
                var productDtos = _mapper.Map<List<ProductDto>>(products);
                
                // Create paginated result
                var paginatedResult = new PaginatedList<ProductDto>(
                    productDtos, 
                    totalCount, 
                    request.PageNumber, 
                    request.PageSize);

                _logger.LogInformation("Successfully retrieved {Count} products", productDtos.Count);
                return Result.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products: {ErrorMessage}", ex.Message);
                return Result.Error($"An error occurred while fetching products: {ex.Message}");
            }
        }
    }
}
