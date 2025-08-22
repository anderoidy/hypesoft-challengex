using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Roles
{
    /// <summary>
    /// Command CQRS para criação de uma nova função (role).
    /// </summary>
    public record CreateRoleCommand(
        string Name,
        string? Description = null,
        string? CreatedBy = null
    ) : IRequest<Result<Guid>>; // ← MUDANÇA: Retorna Guid ao invés da entidade
}
