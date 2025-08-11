using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para atualização de uma Tag existente.
    /// </summary>
    public record UpdateTagCommand(
        Guid Id,
        string Name,
        string? Description = null,
        string? Icon = null,
        string? Color = null,
        bool? IsActive = null,
        int? DisplayOrder = null,
        string? UserId = null
    ) : IRequest<Result<Tag>>;
}
