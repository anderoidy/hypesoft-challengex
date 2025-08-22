using Ardalis.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Hypesoft.Application.Commands.Roles
{
    /// <summary>
    /// Command CQRS para remover claim de uma Role.
    /// </summary>
    public sealed record RemoveClaimFromRoleCommand : IRequest<Result>
    {
        [Required(ErrorMessage = "Role ID is required")]
        public Guid RoleId { get; init; }

        [Required(ErrorMessage = "Claim type is required")]
        public string ClaimType { get; init; } = string.Empty;

        [Required(ErrorMessage = "Claim value is required")]
        public string ClaimValue { get; init; } = string.Empty;

        public string? ModifiedBy { get; init; }

        public RemoveClaimFromRoleCommand(
            Guid roleId,
            string claimType,
            string claimValue,
            string? modifiedBy = null)
        {
            RoleId = roleId;
            ClaimType = claimType;
            ClaimValue = claimValue;
            ModifiedBy = modifiedBy;
        }
    }
}
