using System;
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
}
