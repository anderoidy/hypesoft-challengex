using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Application.Handlers
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<ApplicationRole>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public CreateRoleCommandHandler(
            IRoleRepository roleRepository,
            RoleManager<ApplicationRole> roleManager)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Result<ApplicationRole>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verifica se já existe uma função com o mesmo nome
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null)
                {
                    return Result<ApplicationRole>.Error($"Já existe uma função com o nome '{request.Name}'.");
                }

                // Cria uma nova função
                var role = new ApplicationRole(request.Name)
                {
                    Description = request.Description,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = request.CreatedBy ?? "System"
                };

                // Salva a função no repositório
                var result = await _roleManager.CreateAsync(role);
                
                if (!result.Succeeded)
                {
                    return Result<ApplicationRole>.Error("Falha ao criar a função: " + string.Join(", ", result.Errors));
                }

                return Result<ApplicationRole>.Success(role);
            }
            catch (Exception ex)
            {
                return Result<ApplicationRole>.Error($"Erro ao criar a função: {ex.Message}");
            }
        }
    }
}
