using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.API.Controllers;
using Hypesoft.Application.Commands;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Hypesoft.Application.Common.Interfaces;
using Xunit;

namespace Hypesoft.UnitTests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Product 1" },
                new() { Id = Guid.NewGuid(), Name = "Product 2" }
            };
            var paginatedResult = new PaginatedList<ProductDto>(products, 2, 1, 10);
            var result = Result<PaginatedList<ProductDto>>.Success(paginatedResult);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetAll(1, 10, "name", "asc");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnValue = Assert.IsType<PaginatedList<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnValue.TotalCount);
        }

        [Fact]
        public async Task GetAll_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAll(1, 10, "name", "asc");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new ProductDto { Id = productId, Name = "Test Product" };
            var result = Result<ProductDto>.Success(product);
            
            _mediatorMock.Setup(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(productId, returnValue.Id);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var result = Result<ProductDto>.NotFound();
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetById(productId);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
        {
            // Arrange
            var command = new CreateProductCommand { Name = "New Product", Price = 9.99m };
            var productId = Guid.NewGuid();
            var productDto = new ProductDto { Id = productId, Name = command.Name, Price = command.Price };
            var result = Result<ProductDto>.Success(productDto);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Create(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal(nameof(ProductsController.GetById), createdAtActionResult.ActionName);
            Assert.Equal(productId, ((ProductDto)createdAtActionResult.Value).Id);
        }

        [Fact]
        public async Task Create_WithInvalidProduct_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateProductCommand { Name = "" }; // Nome inválido
            var validationErrors = new List<ValidationError>
            {
                new ValidationError("Name", "Name is required")
            };
            var result = Result<ProductDto>.Invalid(validationErrors);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Create(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var returnValue = Assert.IsType<List<ValidationError>>(badRequestResult.Value);
            Assert.Single(returnValue);
        }

        #endregion
    }
}
