using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para adicionar uma claim a uma função (role).
    /// </summary>
    public record AddClaimToRoleCommand(
        Guid RoleId,
        string ClaimType,
        string ClaimValue
    ) : IRequest<Result>;
}
