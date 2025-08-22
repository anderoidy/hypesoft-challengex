using System;
using System.Linq;
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
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<CreateRoleCommandHandler> _logger;

        public CreateRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            ILogger<CreateRoleCommandHandler> logger
        )
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(
            CreateRoleCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Creating new role: {RoleName}", request.Name);

                // Check if role already exists
                var roleExists = await _roleManager.RoleExistsAsync(request.Name);
                if (roleExists)
                {
                    _logger.LogWarning("Role with name {RoleName} already exists", request.Name);
                    return Result<Guid>.Conflict($"Role with name '{request.Name}' already exists");
                }

                // Create new role
                var role = new ApplicationRole
                {
                    Name = request.Name,
                    Description = request.Description,
                    CreatedBy = request.CreatedBy ?? "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                // Save the role
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError(
                        "Failed to create role {RoleName}: {Errors}",
                        request.Name,
                        errors
                    );
                    return Result<Guid>.Error($"Failed to create role: {errors}");
                }

                _logger.LogInformation(
                    "Successfully created role {RoleName} with ID {RoleId}",
                    role.Name,
                    role.Id
                );
                // ✅ VERSÃO MAIS SEGURA:
                if (Guid.TryParse(role.Id.ToString(), out var roleGuid))
                {
                    _logger.LogInformation(
                        "Successfully created role {RoleName} with ID {RoleId}",
                        role.Name,
                        roleGuid
                    );
                    return Result<Guid>.Success(roleGuid);
                }
                else
                {
                    _logger.LogError("Failed to parse role ID {RoleId} as Guid", role.Id);
                    return Result<Guid>.Error("Failed to parse role ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating role {RoleName}: {ErrorMessage}",
                    request.Name,
                    ex.Message
                );
                return Result<Guid>.Error(
                    $"An error occurred while creating the role: {ex.Message}"
                );
            }
        }
    }
}
