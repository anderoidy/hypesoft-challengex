using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Products
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<CreateProductCommandHandler> logger
        )
        {
            _productRepository =
                productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation(
                    "🔄 Iniciando criação do produto: {ProductName}",
                    request.Name
                );

                // Valida dados básicos
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("❌ Nome do produto é obrigatório");
                    return Result<Guid>.Invalid(
                        new[] { new ValidationError("O nome do produto é obrigatório") }
                    );
                }

                if (request.Price < 0)
                {
                    _logger.LogWarning(
                        "❌ Preço do produto não pode ser negativo: {Price}",
                        request.Price
                    );
                    return Result<Guid>.Invalid(
                        new[] { new ValidationError("O preço do produto não pode ser negativo") }
                    );
                }

                _logger.LogInformation("✅ Validações básicas passaram");

                // Verifica se a categoria existe
                _logger.LogInformation(
                    "🔍 Verificando se categoria {CategoryId} existe",
                    request.CategoryId
                );
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId);

                if (category == null)
                {
                    _logger.LogWarning(
                        "❌ Categoria com ID {CategoryId} não encontrada",
                        request.CategoryId
                    );
                    return Result<Guid>.NotFound(
                        $"Categoria com ID {request.CategoryId} não encontrada"
                    );
                }

                _logger.LogInformation("✅ Categoria {CategoryName} encontrada", category.Name);

                // Mapeia o comando para entidade
                Product product;
                try
                {
                    _logger.LogInformation("🔄 Mapeando CreateProductCommand para Product");
                    product = _mapper.Map<Product>(request);
                    _logger.LogInformation("✅ Mapeamento realizado com sucesso");
                }
                catch (Exception mapEx)
                {
                    _logger.LogError(mapEx, "❌ Erro ao mapear CreateProductCommand para Product");
                    return Result<Guid>.Error($"Erro no mapeamento: {mapEx.Message}");
                }

                // Campos de auditoria
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("🔄 Adicionando produto no repositório");

                // Adiciona no repositório
                try
                {
                    await _productRepository.AddAsync(product);
                    _logger.LogInformation("✅ Produto adicionado no repositório com sucesso");
                }
                catch (Exception repoEx)
                {
                    _logger.LogError(repoEx, "❌ Erro ao adicionar produto no repositório");
                    return Result<Guid>.Error($"Erro no repositório: {repoEx.Message}");
                }

                _logger.LogInformation("✅ Produto criado com sucesso! ID: {ProductId}", product.Id);
                return Result<Guid>.Success(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Erro inesperado ao criar produto");
                return Result<Guid>.Error($"Erro inesperado: {ex.Message}");
            }
        }
    }
}
