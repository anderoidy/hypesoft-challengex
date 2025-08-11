using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace Hypesoft.Application.Queries
{
    /// <summary>
    /// Query CQRS para obter as tags mais populares.
    /// </summary>
    public record GetPopularTagsQuery(
        int Count = 10,
        bool OnlyActive = true
    ) : IRequest<Result<IEnumerable<Tag>>>;
}
