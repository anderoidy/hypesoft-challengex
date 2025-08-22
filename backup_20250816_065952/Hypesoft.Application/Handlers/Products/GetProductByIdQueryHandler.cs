using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public sealed class GetProductByIdQueryHandler 
        : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IApplicationUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandler(
            IApplicationUnitOfWork uow,
            IMapper mapper,
            ILogger<GetProductByIdQueryHandler> logger
        )
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<ProductDto>> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Fetching product with ID {ProductId}", request.Id);

                var product = await _uow.Products.GetByIdAsync(request.Id, cancellationToken);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
                    return Result.NotFound($"Product with ID {request.Id} not found");
                }

                // Map to DTO
                var productDto = _mapper.Map<ProductDto>(product);

                _logger.LogInformation("Successfully retrieved product with ID {ProductId}", request.Id);
                return Result.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with ID {ProductId}: {ErrorMessage}", 
                    request.Id, ex.Message);
                return Result.Error($"An error occurred while fetching the product: {ex.Message}");
            }
        }
    }
}
