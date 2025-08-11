using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;
using Hypesoft.Application.Common.Models;

namespace Hypesoft.Application.Queries;

/// <summary>
/// Query para obter uma lista paginada de produtos com suporte a busca
/// </summary>
public class GetAllProductsQuery : IRequest<Result<PaginatedList<ProductDto>>>
{
    /// <summary>
    /// Termo de busca opcional para filtrar produtos por nome ou descrição
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

    /// <summary>
    /// Inicializa uma nova instância da query de listagem de produtos
    /// </summary>
    /// <param name="searchTerm">Termo de busca opcional</param>
    /// <param name="pageNumber">Número da página (baseado em 1)</param>
    /// <param name="pageSize">Número de itens por página</param>
    public GetAllProductsQuery(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        SearchTerm = searchTerm;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
