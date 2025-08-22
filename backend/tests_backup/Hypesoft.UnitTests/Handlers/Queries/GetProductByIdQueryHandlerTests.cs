using Ardalis.Specification.EntityFrameworkCore;
using AutoFixture;
using FluentAssertions;
using Hypesoft.Application.Handlers.Products;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hypesoft.UnitTests.Handlers.Queries
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<RepositoryBase<Product>> _mockProductRepository;
        private readonly Mock<ILogger<GetProductByIdQueryHandler>> _mockLogger;
        private readonly GetProductByIdQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetProductByIdQueryHandlerTests()
        {
            _mockProductRepository = new Mock<RepositoryBase<Product>>();
            _mockLogger = new Mock<ILogger<GetProductByIdQueryHandler>>();

            _handler = new GetProductByIdQueryHandler(
                _mockProductRepository.Object,
                _mockLogger.Object
            );

            _fixture = new Fixture();
        }

        [Fact]
        public async Task Handle_WithExistingProduct_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = _fixture.Build<Product>().With(x => x.Id, productId).Create();

            var query = new GetProductByIdQuery(productId);

            _mockProductRepository
                .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(productId);
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            _mockProductRepository
                .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
        }
    }
}
