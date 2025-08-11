using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
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

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IApplicationUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _mockLogger;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IApplicationUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
        
        _handler = new CreateProductCommandHandler(
            _mockUnitOfWork.Object, 
            _mockMapper.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = ProductTestData.CreateValidCreateProductCommand();
        var category = ProductTestData.CreateValidCategory();
        var product = ProductTestData.CreateValidProduct();
        
        _mockUnitOfWork.Setup(u => u.Products.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _mockUnitOfWork.Setup(u => u.Products.IsBarcodeUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
            
        _mockUnitOfWork.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = ProductTestData.CreateValidCreateProductCommand();
        
        _mockUnitOfWork.Setup(u => u.Products.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "SKU já existe.");
        
        _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateBarcode_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = ProductTestData.CreateValidCreateProductCommand();
        
        _mockUnitOfWork.Setup(u => u.Products.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _mockUnitOfWork.Setup(u => u.Products.IsBarcodeUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Código de barras já existe.");
        
        _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = ProductTestData.CreateValidCreateProductCommand();
        
        _mockUnitOfWork.Setup(u => u.Products.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _mockUnitOfWork.Setup(u => u.Products.IsBarcodeUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Categoria não encontrada.");
        
        _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndReturnErrorResult()
    {
        // Arrange
        var command = ProductTestData.CreateValidCreateProductCommand();
        var exception = new Exception("Database error");
        
        _mockUnitOfWork.Setup(u => u.Products.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Erro inesperado ao criar produto.");
        
        // Verifica se o erro foi logado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao criar produto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
