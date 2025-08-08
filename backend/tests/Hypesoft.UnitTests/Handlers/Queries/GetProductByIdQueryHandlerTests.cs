using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers;
using Hypesoft.Application.Queries;
using Hypesoft.Application.UnitTests.TestData;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hypesoft.Application.UnitTests.Handlers.Queries;

/// <summary>
/// Testes unitários para o GetProductByIdQueryHandler.
/// Este conjunto de testes verifica o comportamento do manipulador de consulta de produto por ID.
/// </summary>
public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IApplicationUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetProductByIdQueryHandler>> _mockLogger;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IApplicationUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GetProductByIdQueryHandler>>();
        
        _handler = new GetProductByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// Testa a busca bem-sucedida de um produto por ID.
    /// Verifica se o produto é encontrado e mapeado corretamente para o DTO.
    /// </summary>
    [Fact]
    public async Task Handle_WithExistingProduct_ShouldReturnProductDto()
    {
        // Arrange - Configuração do teste
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        // Cria um produto de teste
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        product.CategoryId = categoryId;
        
        // Cria uma categoria de teste
        var category = ProductTestData.CreateValidCategory();
        category.Id = categoryId;
        
        // Configura o mock do repositório de produtos
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        // Configura o mock do repositório de categorias
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
            
        // Configura o mock do AutoMapper para retornar um DTO de produto
        var expectedDto = new ProductDto
        {
            Id = productId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            Sku = product.Sku,
            Barcode = product.Barcode,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            Weight = product.Weight,
            Height = product.Height,
            Width = product.Width,
            Length = product.Length,
            IsFeatured = product.IsFeatured,
            IsPublished = product.IsPublished,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
        
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
            .Returns(expectedDto);
        
        var query = new GetProductByIdQuery(productId);

        // Act - Executa o método que está sendo testado
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Verifica os resultados
        // 1. Verifica se o resultado é um sucesso
        result.IsSuccess.Should().BeTrue();
        
        // 2. Verifica se o DTO retornado contém os dados corretos
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(productId);
        result.Value.Name.Should().Be(product.Name);
        result.Value.CategoryId.Should().Be(categoryId);
        result.Value.CategoryName.Should().Be(category.Name);
        
        // 3. Verifica se os métodos do repositório foram chamados corretamente
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        
        // 4. Verifica se o log de informação foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Buscando produto com ID {productId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testa a busca por um produto que não existe.
    /// Verifica se o manipulador retorna um resultado NotFound.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        // Configura o mock para retornar null quando GetByIdAsync for chamado
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);
            
        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // 1. Verifica se o resultado indica que o produto não foi encontrado
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        
        // 2. Verifica se a mensagem de erro está correta
        result.Errors.Should().Contain(e => e == $"Produto com ID {productId} não encontrado.");
        
        // 3. Verifica se o método do repositório foi chamado corretamente
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        
        // 4. Verifica se o método de busca de categoria não foi chamado
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // 5. Verifica se o log de aviso foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Produto com ID {productId} não encontrado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testa o tratamento de exceções durante a busca de um produto.
    /// Verifica se o manipulador captura a exceção, registra o erro e retorna um resultado de erro.
    /// </summary>
    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndReturnError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var exception = new Exception("Erro de banco de dados");
        
        // Configura o mock para lançar uma exceção quando GetByIdAsync for chamado
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
            
        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // 1. Verifica se o resultado indica um erro
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        
        // 2. Verifica se a mensagem de erro está correta
        result.Errors.Should().Contain(e => e == $"Ocorreu um erro ao buscar o produto com ID {productId}.");
        
        // 3. Verifica se o log de erro foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Erro ao buscar produto com ID {productId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testa a busca de um produto quando a categoria não é encontrada.
    /// Verifica se o manipulador retorna o produto mesmo sem a categoria.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldStillReturnProductWithoutCategoryName()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        // Cria um produto de teste
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        product.CategoryId = categoryId;
        
        // Configura o mock do repositório de produtos
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        // Configura o mock do repositório de categorias para retornar null (categoria não encontrada)
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);
            
        // Configura o mock do AutoMapper para retornar um DTO de produto
        var expectedDto = new ProductDto
        {
            Id = productId,
            Name = product.Name,
            CategoryId = categoryId,
            // CategoryName deve ser null já que a categoria não foi encontrada
        };
        
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
            .Returns(expectedDto);
        
        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // 1. Verifica se o resultado é um sucesso
        result.IsSuccess.Should().BeTrue();
        
        // 2. Verifica se o DTO retornado contém os dados corretos
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(productId);
        result.Value.CategoryId.Should().Be(categoryId);
        result.Value.CategoryName.Should().BeNull(); // A categoria não foi encontrada
        
        // 3. Verifica se os métodos do repositório foram chamados corretamente
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
