using System;
using System.Collections.Generic;
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

namespace Hypesoft.Application.UnitTests.Handlers.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IApplicationUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _mockLogger;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IApplicationUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateProductCommandHandler>>();
        
        _handler = new UpdateProductCommandHandler(
            _mockUnitOfWork.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProductAndReturnSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var category = ProductTestData.CreateValidCategory();
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        
        var command = ProductTestData.CreateValidUpdateProductCommand(productId, category.Id);
        
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
            
        _mockUnitOfWork.Setup(u => u.Tags.GetTagsByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Tag> { ProductTestData.CreateValidTag() });
            
        _mockUnitOfWork.Setup(u => u.Products.Update(It.IsAny<Product>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Products.Update(It.Is<Product>(p => 
            p.Name == command.Name && 
            p.Description == command.Description &&
            p.Price == command.Price &&
            p.StockQuantity == command.StockQuantity &&
            p.UpdatedAt != null)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("successfully updated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = ProductTestData.CreateValidUpdateProductCommand(productId, Guid.NewGuid());
        
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Products.Update(It.IsAny<Product>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        
        var command = ProductTestData.CreateValidUpdateProductCommand(productId, categoryId);
        
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Category not found");
        
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Products.Update(It.IsAny<Product>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentTags_ShouldReturnInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var category = ProductTestData.CreateValidCategory();
        var product = ProductTestData.CreateValidProduct();
        product.Id = productId;
        
        var tagIds = new List<Guid> { Guid.NewGuid() };
        var command = ProductTestData.CreateValidUpdateProductCommand(productId, category.Id, tagIds);
        
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _mockUnitOfWork.Setup(u => u.Categories.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
            
        _mockUnitOfWork.Setup(u => u.Tags.GetTagsByIdsAsync(tagIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Tag>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "One or more tags not found");
        
        _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Categories.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Tags.GetTagsByIdsAsync(tagIds, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Products.Update(It.IsAny<Product>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndReturnError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = ProductTestData.CreateValidUpdateProductCommand(productId, Guid.NewGuid());
        var exception = new Exception("Database error");
        
        _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e == $"An error occurred while updating product with ID {productId}");
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error updating product")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
