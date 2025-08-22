using Ardalis.Result;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Common;
using MediatR;

namespace Hypesoft.Application.Queries.Categories
{
    /// <summary>
    /// Query para obter uma lista paginada de categorias
    /// </summary>
    public class GetAllCategoriesQuery : IRequest<Result<PaginatedList<CategoryDto>>>
    {
        /// <summary>
        /// Termo de busca opcional para filtrar categorias por nome
        /// </summary>
        public string? SearchTerm { get; }

        /// <summary>
        /// Número da página (baseado em 1)
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Número de itens por página
        /// </summary>
        public int PageSize { get; }

        public GetAllCategoriesQuery(
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
