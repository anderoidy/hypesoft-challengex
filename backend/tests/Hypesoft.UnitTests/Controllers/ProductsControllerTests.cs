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
            var actionResult = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnValue = Assert.IsType<Result<PaginatedList<ProductDto>>>(okResult.Value);
            Assert.Equal(2, returnValue.Value.TotalCount);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAll_WithSearchTerm_ReturnsFilteredResults()
        {
            // Arrange
            const string searchTerm = "test";
            var products = new List<ProductDto> { new() { Id = Guid.NewGuid(), Name = "Test Product" } };
            var paginatedResult = new PaginatedList<ProductDto>(products, 1, 1, 10);
            var result = Result<PaginatedList<ProductDto>>.Success(paginatedResult);
            
            _mediatorMock.Setup(m => m.Send(It.Is<GetAllProductsQuery>(q => q.SearchTerm == searchTerm), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetAll(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnValue = Assert.IsType<Result<PaginatedList<ProductDto>>>(okResult.Value);
            Assert.Single(returnValue.Value);
            _mediatorMock.Verify(m => m.Send(It.Is<GetAllProductsQuery>(q => q.SearchTerm == searchTerm), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving products")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
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
            var returnValue = Assert.IsType<Result<ProductDto>>(okResult.Value);
            Assert.Equal(productId, returnValue.Value.Id);
            _mediatorMock.Verify(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()), Times.Once);
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
            Assert.IsType<NotFoundObjectResult>(actionResult);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new CreateProductCommand { Name = "New Product", Price = 100m, CategoryId = Guid.NewGuid() };
            var result = Result<Guid>.Success(productId);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Create(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal(nameof(ProductsController.GetById), createdAtActionResult.ActionName);
            Assert.Equal(productId, ((dynamic)createdAtActionResult.Value).id);
            _mediatorMock.Verify(m => m.Send(It.Is<CreateProductCommand>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            SetupModelStateError("Name", "Required");
            var command = new CreateProductCommand();

            // Act
            var result = await _controller.Create(command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_WithValidationErrors_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateProductCommand { Name = "Test", Price = -1 };
            var validationErrors = new List<ValidationError> { new() { Identifier = "Price", ErrorMessage = "Price must be greater than 0" } };
            var result = Result<Guid>.Invalid(validationErrors);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Create(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.IsType<List<ValidationError>>(badRequestResult.Value);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Updated Product", Price = 150m, CategoryId = Guid.NewGuid() };
            var result = Result.Success();
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Update(productId, command);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            _mediatorMock.Verify(m => m.Send(It.Is<UpdateProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_WithMismatchedIds_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateProductCommand { Id = Guid.NewGuid(), Name = "Updated Product" };

            // Act
            var result = await _controller.Update(Guid.NewGuid(), command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID in the URL does not match the ID in the request body", badRequestResult.Value);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Update_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var productId = Guid.NewGuid();
            SetupModelStateError("Name", "Required");
            var command = new UpdateProductCommand { Id = productId };

            // Act
            var result = await _controller.Update(productId, command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var result = Result.Success();
            
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            _mediatorMock.Verify(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var result = Result.NotFound();
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        #endregion

        private void SetupModelStateError(string key, string errorMessage)
        {
            _controller.ModelState.AddModelError(key, errorMessage);
        }
    }
}
