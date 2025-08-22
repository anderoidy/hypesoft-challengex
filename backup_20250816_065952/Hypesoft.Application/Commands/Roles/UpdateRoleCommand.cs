using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Roles
{
    /// <summary>
    /// Command CQRS para atualização de uma função (role) existente.
    /// </summary>
    public record UpdateRoleCommand(
        Guid Id,
        string Name,
        string? Description = null,
        string? ModifiedBy = null
    ) : IRequest<Result<Guid>>;
}
