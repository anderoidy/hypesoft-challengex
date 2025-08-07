using Hypesoft.Application.Commands;
using FluentValidation;

namespace Hypesoft.Application.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Sku)
            .MaximumLength(50);

        RuleFor(x => x.Barcode)
            .MaximumLength(50);
    }
}
