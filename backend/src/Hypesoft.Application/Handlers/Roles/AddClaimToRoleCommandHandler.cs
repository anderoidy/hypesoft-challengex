using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Roles;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Roles
{
    public class AddClaimToRoleCommandHandler : IRequestHandler<AddClaimToRoleCommand, Result>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AddClaimToRoleCommandHandler> _logger;

        public AddClaimToRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            ILogger<AddClaimToRoleCommandHandler> logger
        )
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(
            AddClaimToRoleCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation(
                    "Adding claim {ClaimType} with value {ClaimValue} to role {RoleId}",
                    request.ClaimType,
                    request.ClaimValue,
                    request.RoleId
                );

                // Find the role by ID
                var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", request.RoleId);
                    return Result.NotFound($"Role with ID {request.RoleId} not found");
                }

                // Check if claim already exists
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                if (
                    existingClaims.Any(c =>
                        c.Type == request.ClaimType && c.Value == request.ClaimValue
                    )
                )
                {
                    _logger.LogWarning(
                        "Claim {ClaimType} with value {ClaimValue} already exists for role {RoleName}",
                        request.ClaimType,
                        request.ClaimValue,
                        role.Name
                    );
                    return Result.Error("This claim already exists for the specified role");
                }

                // Add the claim
                var claim = new Claim(request.ClaimType, request.ClaimValue);
                var result = await _roleManager.AddClaimAsync(role, claim);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError(
                        "Failed to add claim to role {RoleName}: {Errors}",
                        role.Name,
                        errors
                    );
                    return Result.Error($"Failed to add claim to role: {errors}");
                }

                _logger.LogInformation(
                    "Successfully added claim {ClaimType} to role {RoleName}",
                    request.ClaimType,
                    role.Name
                );
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding claim to role {RoleId}: {ErrorMessage}",
                    request.RoleId,
                    ex.Message
                );
                return Result.Error($"An error occurred while adding the claim: {ex.Message}");
            }
        }
    }
}
