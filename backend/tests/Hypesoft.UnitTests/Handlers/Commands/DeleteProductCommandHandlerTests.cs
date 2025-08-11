using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Handlers;
using Hypesoft.Application.UnitTests.TestData;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Hypesoft.Application.Common.Interfaces;

namespace Hypesoft.Application.UnitTests.Handlers.Commands;

/// <summary>
/// Testes unitários para o DeleteProductCommandHandler.
/// Este conjunto de testes verifica o comportamento do manipulador de exclusão de produtos.
/// </summary>
public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IApplicationUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<DeleteProductCommandHandler>> _mockLogger;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IApplicationUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteProductCommandHandler>>();
        
        _handler = new DeleteProductCommandHandler(
            _mockUnitOfWork.Object, 
            _mockLogger.Object);
    }

    /// <summary>
    /// Testa a exclusão bem-sucedida de um produto existente.
    /// Verifica se o produto é recuperado, excluído e as alterações são salvas.
    /// </summary>
    [Fact]
    public async Task Handle_WithExistingProduct_ShouldDeleteProductAndReturnSuccess()
    {
        // Arrange - Configuração do teste
        var productId = Guid.NewGuid();
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        
        var command = ProductTestData.CreateDeleteProductCommand(productId);
        
        // Configura o mock para retornar o produto quando GetByIdAsync for chamado
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        // Configura o mock para retornar uma tarefa concluída quando DeleteAsync for chamado
        _mockUnitOfWork.Setup(u => u.Products.DeleteAsync(product, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        // Configura o mock para retornar uma tarefa concluída quando SaveChangesAsync for chamado
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act - Executa o método que está sendo testado
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verifica os resultados
        // 1. Verifica se o resultado é um sucesso
        result.IsSuccess.Should().BeTrue();
        
        // 2. Verifica se os métodos do repositório foram chamados corretamente
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Products.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // 3. Verifica se o log de informação foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("was deleted")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testa a tentativa de exclusão de um produto que não existe.
    /// Verifica se o manipulador retorna um resultado NotFound.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = ProductTestData.CreateDeleteProductCommand(productId);
        
        // Configura o mock para retornar null quando GetByIdAsync for chamado (produto não encontrado)
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 1. Verifica se o resultado indica que o produto não foi encontrado
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        
        // 2. Verifica se a mensagem de erro está correta
        result.Errors.Should().Contain(e => e == $"Product with ID {productId} not found");
        
        // 3. Verifica se os métodos de exclusão e salvamento não foram chamados
        _mockUnitOfWork.Verify(u => u.Products.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        // 4. Verifica se o log de aviso foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found for deletion")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testa o tratamento de exceções durante a exclusão de um produto.
    /// Verifica se o manipulador captura a exceção, registra o erro e retorna um resultado de erro.
    /// </summary>
    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndReturnError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = ProductTestData.CreateDeleteProductCommand(productId);
        var exception = new Exception("Erro de banco de dados");
        
        // Configura o mock para lançar uma exceção quando GetByIdAsync for chamado
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 1. Verifica se o resultado indica um erro
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        
        // 2. Verifica se a mensagem de erro está correta
        result.Errors.Should().Contain(e => e == $"An error occurred while deleting product with ID {productId}");
        
        // 3. Verifica se o log de erro foi registrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting product")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        
        // 4. Verifica se os métodos de exclusão e salvamento não foram chamados
        _mockUnitOfWork.Verify(u => u.Products.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
