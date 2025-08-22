using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Hypesoft.Application.Common.Dtos;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Roles;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Roles
{
    public class GetAllRolesQueryHandler
        : IRequestHandler<GetAllRolesQuery, Result<PaginatedList<RoleDto>>>
    {
        private readonly RepositoryBase<ApplicationRole> _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;

        public GetAllRolesQueryHandler(
            RepositoryBase<ApplicationRole> roleRepository,
            IMapper mapper,
            ILogger<GetAllRolesQueryHandler> logger
        )
        {
            _roleRepository =
                roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<PaginatedList<RoleDto>>> Handle(
            GetAllRolesQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation(
                    "Fetching roles with page {PageNumber} and page size {PageSize}",
                    request.PageNumber,
                    request.PageSize
                );

                // Create specification for filtering and pagination
                var spec = new RoleSpecification(
                    searchTerm: request.SearchTerm,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize
                );

                // Get paginated result usando RepositoryBase
                var roles = await _roleRepository.ListAsync(spec, cancellationToken);
                var totalCount = await _roleRepository.CountAsync(spec, cancellationToken);

                // Map to DTOs
                var roleDtos = roles.Select(role => _mapper.Map<RoleDto>(role)).ToList();

                var paginatedResult = new PaginatedList<RoleDto>(
                    roleDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                _logger.LogInformation("Successfully retrieved {Count} roles", roleDtos.Count);
                return Result<PaginatedList<RoleDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles: {ErrorMessage}", ex.Message);
                return Result<PaginatedList<RoleDto>>.Error(
                    $"An error occurred while fetching roles: {ex.Message}"
                );
            }
        }
    }

    public class RoleSpecification : Specification<ApplicationRole>
    {
        public RoleSpecification(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(r =>
                    r.Name!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    || (
                        !string.IsNullOrEmpty(r.Description)
                        && r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    )
                );
            }

            // Apply ordering
            Query.OrderBy(r => r.Name);

            // Apply pagination
            Query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}
