using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class GetProductsReportQueryHandler
        : IRequestHandler<GetProductsReportQuery, Result<ProductsReportDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetProductsReportQueryHandler> _logger;

        public GetProductsReportQueryHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<GetProductsReportQueryHandler> logger
        )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<Result<ProductsReportDto>> Handle(
            GetProductsReportQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();

                var categoryDict = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

                var reportItems = products
                    .Select(p => new ProductReportItemDto
                    {
                        Id = p.Id.ToString(),
                        Name = p.Name,
                        CategoryName = categoryDict.GetValueOrDefault(
                            p.CategoryId.ToString(),
                            "Sem categoria"
                        ),
                        Price = p.Price,
                        Stock = p.StockQuantity, // ✅ CORRIGIDO: Usando StockQuantity
                        Status = p.StockQuantity < 10 ? "Estoque Baixo" : "Em Estoque", // ✅ CORRIGIDO
                    })
                    .ToList();

                var report = new ProductsReportDto
                {
                    Products = reportItems,
                    TotalProducts = products.Count(),
                    TotalValue = products.Sum(p => p.Price * p.StockQuantity), // ✅ CORRIGIDO
                    LowStockProducts = products.Count(p => p.StockQuantity < 10), // ✅ CORRIGIDO
                    ProductsByCategory = reportItems
                        .GroupBy(p => p.CategoryName)
                        .ToDictionary(g => g.Key, g => g.Count()),
                };

                return Result.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de produtos");
                return Result.Error("Erro ao gerar relatório");
            }
        }
    }
}
