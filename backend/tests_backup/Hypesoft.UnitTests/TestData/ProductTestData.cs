using System;
using System.Collections.Generic;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Application.UnitTests.TestData;

public static class ProductTestData
{
    public static CreateProductCommand CreateValidCreateProductCommand()
    {
        return new CreateProductCommand(
            Name: "Produto de Teste",
            Description: "Descrição do produto de teste",
            Price: 99.99m,
            CategoryId: Guid.NewGuid(),
            Sku: "SKU12345",
            Barcode: "7891234567890",
            DiscountPrice: 89.99m,
            StockQuantity: 100,
            ImageUrl: "https://example.com/product.jpg",
            Weight: 0.5f,
            Height: 10.0f,
            Width: 5.0f,
            Length: 15.0f,
            IsFeatured: true,
            IsPublished: true,
            UserId: "user123"
        );
    }

    public static UpdateProductCommand CreateValidUpdateProductCommand(Guid productId, Guid categoryId, List<Guid>? tagIds = null)
    {
        return new UpdateProductCommand(
            Id: productId,
            Name: "Produto Atualizado",
            Description: "Descrição atualizada do produto",
            Price: 199.99m,
            StockQuantity: 50,
            ImageUrl: "https://example.com/updated-product.jpg",
            CategoryId: categoryId,
            TagIds: tagIds ?? new List<Guid> { Guid.NewGuid() }
        );
    }

    public static DeleteProductCommand CreateDeleteProductCommand(Guid productId)
    {
        return new DeleteProductCommand(productId);
    }

    public static Category CreateValidCategory()
    {
        return new Category("Categoria de Teste", "Descrição da categoria de teste")
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Product CreateValidProduct()
    {
        var category = CreateValidCategory();
        
        return new Product(
            name: "Produto de Teste",
            description: "Descrição do produto de teste",
            price: 99.99m,
            categoryId: category.Id,
            sku: "SKU12345",
            barcode: "7891234567890",
            discountPrice: 89.99m,
            stockQuantity: 100,
            imageUrl: "https://example.com/product.jpg",
            weight: 0.5f,
            height: 10.0f,
            width: 5.0f,
            length: 15.0f,
            isFeatured: true,
            isPublished: true,
            userId: "user123"
        );
    }

    public static Domain.Entities.Tag CreateValidTag()
    {
        return new Domain.Entities.Tag("Tag de Teste", "Descrição da tag de teste")
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<Product> CreateProductList(int count, Guid? categoryId = null)
    {
        var products = new List<Product>();
        var category = categoryId.HasValue 
            ? new Category("Categoria de Teste", "Descrição") { Id = categoryId.Value }
            : CreateValidCategory();

        for (int i = 0; i < count; i++)
        {
            var product = new Product(
                name: $"Produto de Teste {i + 1}",
                description: $"Descrição do produto de teste {i + 1}",
                price: 10.99m * (i + 1),
                categoryId: category.Id,
                sku: $"SKU-{1000 + i}",
                barcode: $"78912345{1000 + i}",
                discountPrice: 9.99m * (i + 1),
                stockQuantity: 10 * (i + 1),
                imageUrl: $"https://example.com/product-{i + 1}.jpg",
                weight: 0.5f * (i + 1),
                height: 10.0f + i,
                width: 5.0f + i,
                length: 15.0f + i,
                isFeatured: i % 2 == 0,
                isPublished: true,
                userId: "user123")
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                Category = category
            };

            products.Add(product);
        }

        return products;
    }
}
