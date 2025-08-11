using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para exclusão de uma função (role) existente.
    /// </summary>
    public record DeleteRoleCommand(
        Guid Id
    ) : IRequest<Result>;
}
