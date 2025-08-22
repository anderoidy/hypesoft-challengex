using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.Common;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers;
using Hypesoft.Application.Queries;
using Hypesoft.Application.UnitTests.TestData;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Hypesoft.Application.Common.Interfaces;

namespace Hypesoft.Application.UnitTests.Handlers.Queries;

/// <summary>
/// Testes unitários para o GetAllProductsQueryHandler.
/// Este conjunto de testes verifica o comportamento do manipulador de consulta de listagem de produtos.
/// </summary>
public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IApplicationUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetAllProductsQueryHandler>> _mockLogger;
    private readonly GetAllProductsQueryHandler _handler;
    private const int TestPageSize = 10;
    private const int TestPageNumber = 1;
    private const string TestSearchTerm = "teste";

    public GetAllProductsQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IApplicationUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GetAllProductsQueryHandler>>();
        
        _handler = new GetAllProductsQueryHandler(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldReturnPaginatedResult()
    {
        // Arrange
        var testProducts = ProductTestData.CreateProductList(5);
        var testProductDtos = testProducts.Select(p => new ProductDto { Id = p.Id, Name = p.Name }).ToList();
        var totalCount = testProducts.Count;
        
        _mockUnitOfWork.Setup(u => u.Products.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((testProducts, totalCount));
            
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(testProductDtos);
            
        var query = new GetAllProductsQuery(pageNumber: TestPageNumber, pageSize: TestPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(testProducts.Count);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.PageNumber.Should().Be(TestPageNumber);
        result.Value.PageSize.Should().Be(TestPageSize);
        
        _mockUnitOfWork.Verify(
            u => u.Products.GetPagedAsync(
                TestPageNumber, 
                TestPageSize, 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                "Category",
                It.IsAny<CancellationToken>()), 
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Buscando produtos com os parâmetros")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldApplyFilter()
    {
        // Arrange
        var testProducts = ProductTestData.CreateProductList(3);
        var filteredProducts = testProducts.Take(2).ToList(); // Apenas 2 produtos correspondem ao termo de busca
        var testProductDtos = filteredProducts.Select(p => new ProductDto { Id = p.Id, Name = p.Name }).ToList();
        
        _mockUnitOfWork.Setup(u => u.Products.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((filteredProducts, filteredProducts.Count));
            
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(testProductDtos);
            
        var query = new GetAllProductsQuery(
            searchTerm: TestSearchTerm, 
            pageNumber: TestPageNumber, 
            pageSize: TestPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(filteredProducts.Count);
        
        _mockUnitOfWork.Verify(
            u => u.Products.GetPagedAsync(
                TestPageNumber, 
                TestPageSize, 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                "Category",
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoProducts_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyProducts = new List<Product>();
        var emptyDtos = new List<ProductDto>();
        
        _mockUnitOfWork.Setup(u => u.Products.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyProducts, 0));
            
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(emptyDtos);
            
        var query = new GetAllProductsQuery(pageNumber: TestPageNumber, pageSize: TestPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnError()
    {
        // Arrange
        var exception = new Exception("Erro no repositório");
        
        _mockUnitOfWork.Setup(u => u.Products.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<Expression<Func<Product, bool>>>(), 
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
            
        var query = new GetAllProductsQuery(pageNumber: TestPageNumber, pageSize: TestPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao buscar produtos")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
