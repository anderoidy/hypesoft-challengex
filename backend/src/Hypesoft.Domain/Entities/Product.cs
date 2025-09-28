using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Interfaces;

namespace Hypesoft.Domain.Entities
{
    public class Product : BaseEntity, IAggregateRoot
    {
        // Properties
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal Price { get; private set; }
        public decimal? DiscountPrice { get; private set; }
        public int StockQuantity { get; private set; }
        public string? Sku { get; private set; }
        public string? Barcode { get; private set; }
        public bool IsFeatured { get; private set; }
        public bool IsPublished { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public string? Slug { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation
        public Guid CategoryId { get; private set; }
        public virtual Category? Category { get; private set; }

        // Avoid nulls
        public virtual ICollection<Category> Categories { get; private set; } =
            new List<Category>();

        protected Product() { } // EF/Mongo

        public Product(
            string name,
            string? description,
            decimal price,
            Guid categoryId,
            string? sku = null,
            string? barcode = null,
            decimal? discountPrice = null,
            int stockQuantity = 0,
            string? imageUrl = null,
            bool isFeatured = false,
            bool isPublished = false,
            string? slug = null
        )
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetCategory(categoryId);
            SetSku(sku);
            SetBarcode(barcode);
            SetDiscountPrice(discountPrice);
            SetStockQuantity(stockQuantity);
            SetImageUrl(imageUrl);
            SetIsFeatured(isFeatured);
            SetIsPublished(isPublished);

            if (!string.IsNullOrEmpty(name))
            {
                var slugToUse = slug ?? GenerateSlug(name);
                SetSlug(slugToUse);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                SetSlug(slug);
            }
        }

        public void SetUpdatedAt(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }

        // Atualiza campos de auditoria usando BaseEntity
        public void SetLastModifiedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(userId));

            UpdateAuditFields(userId); // seta ModifiedAt/ModifiedBy (BaseEntity)
            UpdatedAt = DateTime.UtcNow; // campo local do Product
        }

        // Domain behavior
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("O nome do produto é obrigatório");

            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
        }

        public void SetPrice(decimal price)
        {
            if (price < 0)
                throw new DomainException("O preço não pode ser negativo");

            Price = price;
        }

        public void SetDiscountPrice(decimal? discountPrice)
        {
            if (discountPrice.HasValue && discountPrice < 0)
                throw new DomainException("O preço com desconto não pode ser negativo");

            DiscountPrice = discountPrice;
        }

        public void SetStockQuantity(int quantity)
        {
            if (quantity < 0)
                throw new DomainException("A quantidade em estoque não pode ser negativa");

            StockQuantity = quantity;
        }

        public void SetSku(string? sku)
        {
            Sku = sku?.Trim();
        }

        public void SetBarcode(string? barcode)
        {
            Barcode = barcode?.Trim();
        }

        public void SetImageUrl(string? imageUrl)
        {
            ImageUrl = imageUrl?.Trim();
        }

        public void SetIsFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
        }

        public void SetIsPublished(bool isPublished)
        {
            IsPublished = isPublished;

            if (isPublished && !PublishedAt.HasValue)
            {
                PublishedAt = DateTime.UtcNow;
            }
            else if (!isPublished)
            {
                PublishedAt = null;
            }
        }

        public void SetSlug(string slug)
        {
            Slug = string.IsNullOrWhiteSpace(slug) ? GenerateSlug(Name) : slug.Trim().ToLower();
            UpdatedAt = DateTime.UtcNow;
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name.ToLower()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("#", "sharp")
                .Replace("+", "plus");
        }

        public void SetCategory(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                throw new DomainException("A categoria é obrigatória");

            CategoryId = categoryId;
        }

        // Business methods
        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("A quantidade deve ser maior que zero");

            StockQuantity += quantity;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("A quantidade deve ser maior que zero");

            if (StockQuantity < quantity)
                throw new DomainException("Quantidade em estoque insuficiente");

            StockQuantity -= quantity;
        }

        public bool IsInStock() => StockQuantity > 0;

        public bool HasDiscount() => DiscountPrice.HasValue && DiscountPrice < Price;

        public decimal GetCurrentPrice() => HasDiscount() ? DiscountPrice!.Value : Price;

        public void Update(
            string name,
            string? description,
            decimal price,
            Guid categoryId,
            string? imageUrl = null,
            int stockQuantity = 0,
            decimal? discountPrice = null,
            bool? isFeatured = null,
            string? userId = null,
            string? slug = null
        )
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetCategory(categoryId);
            SetImageUrl(imageUrl);
            SetStockQuantity(stockQuantity);
            SetDiscountPrice(discountPrice);

            if (!string.IsNullOrEmpty(slug))
            {
                SetSlug(slug);
            }

            if (isFeatured.HasValue)
            {
                SetIsFeatured(isFeatured.Value);
            }

            // Atualiza auditoria via BaseEntity
            if (!string.IsNullOrEmpty(userId))
            {
                SetLastModifiedBy(userId); // chama UpdateAuditFields + atualiza UpdatedAt
            }
        }
    }
}
