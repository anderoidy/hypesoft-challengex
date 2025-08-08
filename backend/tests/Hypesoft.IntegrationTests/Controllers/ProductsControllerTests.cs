using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hypesoft.IntegrationTests.Controllers;

public class ProductsControllerTests : TestBase
{
    private const string BaseUrl = "/api/products";

    [Fact]
    public async Task GetAll_WithoutProducts_ReturnsEmptyList()
    {
        // Arrange - O banco de dados está vazio

        // Act
        var response = await TestClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>.PaginatedData>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WithProducts_ReturnsPaginatedList()
    {
        // Arrange - Adiciona alguns produtos ao banco de dados
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 1",
                Description = "Descrição do produto 1",
                Price = 100.50m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 2",
                Description = "Descrição do produto 2",
                Price = 200.75m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 3",
                Description = "Outra descrição",
                Price = 300.00m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            }
        };

        await DbContext.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act - Busca a primeira página com 2 itens
        var response = await TestClient.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>.PaginatedData>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_WithSearchTerm_ReturnsMatchingProducts()
    {
        // Arrange - Adiciona alguns produtos ao banco de dados
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "iPhone 13",
                Description = "Smartphone da Apple",
                Price = 5000.00m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Samsung Galaxy S21",
                Description = "Smartphone da Samsung",
                Price = 4500.00m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Fone de Ouvido Bluetooth",
                Description = "Acessório para smartphone",
                Price = 250.00m,
                CategoryId = Guid.NewGuid(),
                IsPublished = true
            }
        };

        await DbContext.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act - Busca produtos que contenham "phone" no nome ou descrição
        var response = await TestClient.GetAsync($"{BaseUrl}?search=phone");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>.PaginatedData>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3); // Todos os produtos contêm "phone" no nome ou descrição
        result.TotalCount.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)] // Página inválida
    [InlineData(1, 0)]  // Tamanho de página inválido
    [InlineData(1, 101)] // Tamanho de página maior que o máximo permitido
    public async Task GetAll_WithInvalidPagination_ReturnsBadRequest(int pageNumber, int pageSize)
    {
        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsProduct()
    {
        // Arrange - Cria um produto no banco de dados
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Eletrônicos",
            Description = "Produtos eletrônicos em geral"
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Smartphone Avançado",
            Description = "Smartphone com câmera de alta resolução",
            Price = 3500.00m,
            CategoryId = category.Id,
            IsPublished = true,
            StockQuantity = 10
        };

        await DbContext.Categories.AddAsync(category);
        await DbContext.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act - Busca o produto pelo ID
        var response = await TestClient.GetAsync($"{BaseUrl}/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Price.Should().Be(product.Price);
        result.StockQuantity.Should().Be(product.StockQuantity);
        result.CategoryId.Should().Be(product.CategoryId);
        result.CategoryName.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange - ID que não existe no banco de dados
        var nonExistingId = Guid.NewGuid();

        // Act - Tenta buscar um produto com ID inexistente
        var response = await TestClient.GetAsync($"{BaseUrl}/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange - ID inválido (não um Guid)
        var invalidId = "not-a-valid-guid";

        // Act - Tenta buscar com ID inválido
        var response = await TestClient.GetAsync($"{BaseUrl}/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithUnpublishedProduct_ReturnsNotFound()
    {
        // Arrange - Cria um produto não publicado no banco de dados
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Não Publicado",
            Description = "Este produto não está publicado",
            Price = 100.00m,
            CategoryId = Guid.NewGuid(),
            IsPublished = false,
            StockQuantity = 5
        };

        await DbContext.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act - Tenta buscar um produto não publicado
        var response = await TestClient.GetAsync($"{BaseUrl}/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange - Cria uma categoria no banco de dados
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Eletrônicos",
            Description = "Produtos eletrônicos em geral"
        };

        await DbContext.Categories.AddAsync(category);
        await DbContext.SaveChangesAsync();

        // Dados do novo produto
        var createProductDto = new
        {
            Name = "Novo Smartphone",
            Description = "Smartphone com câmera de alta resolução",
            Price = 2500.00m,
            CategoryId = category.Id,
            StockQuantity = 15,
            Sku = "SMARTPHONE-001",
            Barcode = "1234567890123"
        };

        // Act - Envia a requisição para criar o produto
        var response = await TestClient.PostAsJsonAsync(BaseUrl, createProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verifica o cabeçalho Location
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/products/");
        
        // Verifica o corpo da resposta
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be(createProductDto.Name);
        result.Description.Should().Be(createProductDto.Description);
        result.Price.Should().Be(createProductDto.Price);
        result.CategoryId.Should().Be(createProductDto.CategoryId);
        result.StockQuantity.Should().Be(createProductDto.StockQuantity);
        result.Sku.Should().Be(createProductDto.Sku);
        result.Barcode.Should().Be(createProductDto.Barcode);
        result.IsPublished.Should().BeFalse(); // Por padrão, o produto não deve ser publicado

        // Verifica se o produto foi salvo no banco de dados
        var productInDb = await DbContext.Products.FindAsync(result.Id);
        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be(createProductDto.Name);
    }

    [Fact]
    public async Task Create_WithNonExistingCategory_ReturnsBadRequest()
    {
        // Arrange - Cria um DTO com uma categoria que não existe
        var createProductDto = new
        {
            Name = "Produto com Categoria Inexistente",
            Description = "Este produto não deve ser criado",
            Price = 100.00m,
            CategoryId = Guid.NewGuid(), // ID que não existe no banco de dados
            StockQuantity = 10
        };

        // Act - Tenta criar o produto com categoria inexistente
        var response = await TestClient.PostAsJsonAsync(BaseUrl, createProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verifica se a mensagem de erro é apropriada
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Categoria não encontrada");
    }

    [Theory]
    [MemberData(nameof(GetInvalidProducts))]
    public async Task Create_WithInvalidData_ReturnsBadRequest(object invalidProduct)
    {
        // Act - Tenta criar um produto com dados inválidos
        var response = await TestClient.PostAsJsonAsync(BaseUrl, invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateSku_ReturnsBadRequest()
    {
        // Arrange - Cria uma categoria e um produto com SKU existente
        var category = new Category { Id = Guid.NewGuid(), Name = "Categoria" };
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Existente",
            Sku = "DUPLICATE-SKU",
            CategoryId = category.Id,
            Price = 100.00m,
            IsPublished = true
        };

        await DbContext.Categories.AddAsync(category);
        await DbContext.Products.AddAsync(existingProduct);
        await DbContext.SaveChangesAsync();

        // Tenta criar um novo produto com o mesmo SKU
        var createProductDto = new
        {
            Name = "Novo Produto com SKU Duplicado",
            Description = "Este produto não deve ser criado",
            Price = 200.00m,
            CategoryId = category.Id,
            StockQuantity = 5,
            Sku = "DUPLICATE-SKU" // SKU duplicado
        };

        // Act - Tenta criar o produto com SKU duplicado
        var response = await TestClient.PostAsJsonAsync(BaseUrl, createProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Já existe um produto com este SKU");
    }

    public static IEnumerable<object[]> GetInvalidProducts()
    {
        // Produto sem nome
        yield return new object[] { new { 
            Description = "Produto sem nome", 
            Price = 100.00m, 
            CategoryId = Guid.NewGuid(),
            StockQuantity = 10 
        }};

        // Produto com preço negativo
        yield return new object[] { new { 
            Name = "Produto com preço negativo",
            Description = "Este produto tem preço inválido", 
            Price = -10.00m, 
            CategoryId = Guid.NewGuid(),
            StockQuantity = 5 
        }};

        // Produto sem categoria
        yield return new object[] { new { 
            Name = "Produto sem categoria",
            Description = "Este produto não tem categoria", 
            Price = 50.00m,
            StockQuantity = 2 
        }};

        // Produto com quantidade em estoque negativa
        yield return new object[] { new { 
            Name = "Produto com estoque negativo",
            Description = "Este produto tem quantidade em estoque inválida", 
            Price = 75.50m, 
            CategoryId = Guid.NewGuid(),
            StockQuantity = -5 
        }};
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange - Cria uma categoria e um produto no banco de dados
        var category1 = new Category { Id = Guid.NewGuid(), Name = "Eletrônicos" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Acessórios" };
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Original",
            Description = "Descrição original",
            Price = 100.00m,
            CategoryId = category1.Id,
            StockQuantity = 10,
            Sku = "ORIGINAL-SKU",
            Barcode = "1111111111111",
            IsPublished = false
        };

        await DbContext.Categories.AddRangeAsync(category1, category2);
        await DbContext.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Dados para atualização do produto
        var updateProductDto = new
        {
            Name = "Produto Atualizado",
            Description = "Nova descrição mais detalhada",
            Price = 150.00m,
            CategoryId = category2.Id, // Muda a categoria
            StockQuantity = 20,
            Sku = "UPDATED-SKU",
            Barcode = "2222222222222",
            IsPublished = true
        };

        // Act - Atualiza o produto
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{product.Id}", updateProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verifica o corpo da resposta
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(updateProductDto.Name);
        result.Description.Should().Be(updateProductDto.Description);
        result.Price.Should().Be(updateProductDto.Price);
        result.CategoryId.Should().Be(updateProductDto.CategoryId);
        result.StockQuantity.Should().Be(updateProductDto.StockQuantity);
        result.Sku.Should().Be(updateProductDto.Sku);
        result.Barcode.Should().Be(updateProductDto.Barcode);
        result.IsPublished.Should().Be(updateProductDto.IsPublished);

        // Verifica se o produto foi atualizado no banco de dados
        var productInDb = await DbContext.Products.FindAsync(product.Id);
        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be(updateProductDto.Name);
        productInDb.CategoryId.Should().Be(updateProductDto.CategoryId);
        productInDb.IsPublished.Should().Be(updateProductDto.IsPublished);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange - ID que não existe no banco de dados
        var nonExistingId = Guid.NewGuid();
        var updateProductDto = new { Name = "Produto Inexistente", Price = 100.00m, CategoryId = Guid.NewGuid() };

        // Act - Tenta atualizar um produto que não existe
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{nonExistingId}", updateProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithDuplicateSku_ReturnsBadRequest()
    {
        // Arrange - Cria duas categorias e dois produtos no banco de dados
        var category = new Category { Id = Guid.NewGuid(), Name = "Eletrônicos" };
        var product1 = new Product { Id = Guid.NewGuid(), Name = "Produto 1", Sku = "SKU-001", CategoryId = category.Id, Price = 100.00m };
        var product2 = new Product { Id = Guid.NewGuid(), Name = "Produto 2", Sku = "SKU-002", CategoryId = category.Id, Price = 200.00m };

        await DbContext.Categories.AddAsync(category);
        await DbContext.Products.AddRangeAsync(product1, product2);
        await DbContext.SaveChangesAsync();

        // Tenta atualizar o produto 2 com o SKU do produto 1
        var updateProductDto = new { Name = "Produto 2 Atualizado", Sku = "SKU-001", CategoryId = category.Id, Price = 250.00m };

        // Act - Tenta atualizar com SKU duplicado
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{product2.Id}", updateProductDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Já existe um produto com este SKU");
    }

    [Fact]
    public async Task Update_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - Cria um produto no banco de dados
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Válido",
            Price = 100.00m,
            CategoryId = Guid.NewGuid(),
            IsPublished = true
        };

        await DbContext.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Tenta atualizar com preço negativo (inválido)
        var invalidUpdateDto = new { Name = "Produto Inválido", Price = -10.00m, CategoryId = Guid.NewGuid() };

        // Act - Tenta atualizar com dados inválidos
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{product.Id}", invalidUpdateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithNonExistingCategory_ReturnsBadRequest()
    {
        // Arrange - Cria um produto no banco de dados
        var category = new Category { Id = Guid.NewGuid(), Name = "Categoria Válida" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Original",
            Price = 100.00m,
            CategoryId = category.Id,
            IsPublished = true
        };

        await DbContext.Categories.AddAsync(category);
        await DbContext.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Tenta atualizar com uma categoria que não existe
        var nonExistingCategoryId = Guid.NewGuid();
        var updateDto = new { Name = "Produto Atualizado", Price = 150.00m, CategoryId = nonExistingCategoryId };

        // Act - Tenta atualizar com categoria inexistente
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{product.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Categoria não encontrada");
    }
}
