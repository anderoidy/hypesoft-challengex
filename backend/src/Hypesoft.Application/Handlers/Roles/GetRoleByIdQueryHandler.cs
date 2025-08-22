using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Hypesoft.Application.Common.Dtos;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Roles;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Roles
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly RepositoryBase<ApplicationRole> _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRoleByIdQueryHandler> _logger;

        public GetRoleByIdQueryHandler(
            RepositoryBase<ApplicationRole> roleRepository,
            IMapper mapper,
            ILogger<GetRoleByIdQueryHandler> logger
        )
        {
            _roleRepository =
                roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<RoleDto>> Handle(
            GetRoleByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Fetching role with ID {RoleId}", request.Id);

                // Get the role by ID usando RepositoryBase
                var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", request.Id);
                    return Result.NotFound($"Role with ID {request.Id} not found");
                }

                // Map to DTO
                var roleDto = _mapper.Map<RoleDto>(role);

                _logger.LogInformation("Successfully retrieved role with ID {RoleId}", request.Id);
                return Result.Success(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching role with ID {RoleId}: {ErrorMessage}",
                    request.Id,
                    ex.Message
                );
                return Result.Error($"An error occurred while fetching the role: {ex.Message}");
            }
        }
    }
}
