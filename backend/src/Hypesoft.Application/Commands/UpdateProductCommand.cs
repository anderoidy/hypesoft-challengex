using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands;

public sealed record UpdateProductCommand : IRequest<Result<ProductDto>>
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid Id { get; init; }
    
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; init; }
    
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public decimal Price { get; init; }
    
    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; init; }
    
    [StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
    public string? Sku { get; init; }
    
    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    public string? ImageUrl { get; init; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Discount price cannot be negative")]
    public decimal? DiscountPrice { get; init; }
    
    public bool? IsFeatured { get; init; }
    
    public IReadOnlyList<Guid>? TagIds { get; init; }
    
    public string? UserId { get; init; }
    
    public UpdateProductCommand(
        Guid id,
        string name,
        string? description,
        decimal price,
        Guid categoryId,
        string? sku = null,
        string? imageUrl = null,
        int stockQuantity = 0,
        decimal? discountPrice = null,
        bool? isFeatured = null,
        IReadOnlyList<Guid>? tagIds = null,
        string? userId = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        Sku = sku;
        ImageUrl = imageUrl;
        StockQuantity = stockQuantity;
        DiscountPrice = discountPrice;
        IsFeatured = isFeatured;
        TagIds = tagIds;
        UserId = userId;
    }
}