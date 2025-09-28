using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Roles;

public record RemoveClaimFromRoleCommand(Guid RoleId, string ClaimType, string ClaimValue)
    : IRequest<Result>;
