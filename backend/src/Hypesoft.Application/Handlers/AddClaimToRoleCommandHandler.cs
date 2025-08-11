using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Application.Handlers
{
    public class AddClaimToRoleCommandHandler : IRequestHandler<AddClaimToRoleCommand, Result>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AddClaimToRoleCommandHandler(
            IRoleRepository roleRepository,
            RoleManager<ApplicationRole> roleManager)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Result> Handle(AddClaimToRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a função existente
                var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                if (role == null)
                {
                    return Result.NotFound($"Função com ID {request.RoleId} não encontrada.");
                }

                // Verifica se a claim já existe para esta função
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                if (existingClaims.Any(c => c.Type == request.ClaimType && c.Value == request.ClaimValue))
                {
                    return Result.Error($"A claim '{request.ClaimType}' com valor '{request.ClaimValue}' já existe para esta função.");
                }

                // Cria e adiciona a claim
                var claim = new System.Security.Claims.Claim(request.ClaimType, request.ClaimValue);
                var result = await _roleManager.AddClaimAsync(role, claim);
                
                if (!result.Succeeded)
                {
                    return Result.Error("Falha ao adicionar a claim à função: " + string.Join(", ", result.Errors));
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error($"Erro ao adicionar a claim à função: {ex.Message}");
            }
        }
    }
}
