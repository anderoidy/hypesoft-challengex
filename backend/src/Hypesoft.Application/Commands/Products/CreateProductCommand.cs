// Commands - Comandos CQRS que representam intenções de alteração de estado
using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Products
{
    /// <summary>
    /// Command CQRS para criação de um novo Produto.
    /// </summary>
    public record CreateProductCommand(
        string Name,
        string? Description,
        decimal Price,
        Guid CategoryId,
        string? Sku = null,
        string? Barcode = null,
        decimal? DiscountPrice = null,
        int StockQuantity = 0,
        string? ImageUrl = null,
        float? Weight = null,
        float? Height = null,
        float? Width = null,
        float? Length = null,
        bool IsFeatured = false,
        bool IsPublished = false,
        string? CreatedBy = null  // ✅ Mudança: UserId → CreatedBy
    ) : IRequest<Result<Guid>>;
}
