using MediatR;
using Ardalis.Result;
using System;
using System.Collections.Generic;

namespace Hypesoft.Application.Commands;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl = null,
    Guid? CategoryId = null,
    IReadOnlyList<Guid>? TagIds = null) : IRequest<Result>;