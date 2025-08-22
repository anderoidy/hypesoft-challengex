using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Common.Dtos;
using MediatR;
using Hypesoft.Domain.Common;

namespace Hypesoft.Application.Queries.Roles
{
    /// <summary>
    /// Query to get a paginated list of roles with optional search functionality
    /// </summary>
    public class GetAllRolesQuery : IRequest<Result<PaginatedList<RoleDto>>>
    {
        /// <summary>
        /// Optional search term to filter roles by name or description
        /// </summary>
        public string? SearchTerm { get; }

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Initializes a new instance of the role list query
        /// </summary>
        /// <param name="searchTerm">Optional search term</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        public GetAllRolesQuery(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
