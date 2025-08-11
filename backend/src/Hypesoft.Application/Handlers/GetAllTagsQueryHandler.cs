using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers
{
    public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<PaginatedList<Tag>>>
    {
        private readonly ITagRepository _tagRepository;

        public GetAllTagsQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<PaginatedList<Tag>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validação dos parâmetros de paginação
                if (request.PageNumber < 1)
                    return Result<PaginatedList<Tag>>.Error("O número da página deve ser maior que zero.");

                if (request.PageSize < 1 || request.PageSize > 100)
                    return Result<PaginatedList<Tag>>.Error("O tamanho da página deve estar entre 1 e 100.");

                // Consulta base
                var query = _tagRepository.GetAll();

                // Aplica filtros
                if (request.IsActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == request.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim().ToLower();
                    query = query.Where(t => 
                        t.Name.ToLower().Contains(searchTerm) || 
                        (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
                }

                // Ordenação
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.SortDescending 
                        ? query.OrderByDescending(t => t.Name) 
                        : query.OrderBy(t => t.Name),
                    "displayorder" => request.SortDescending 
                        ? query.OrderByDescending(t => t.DisplayOrder) 
                        : query.OrderBy(t => t.DisplayOrder),
                    _ => request.SortDescending 
                        ? query.OrderByDescending(t => t.CreatedAt) 
                        : query.OrderBy(t => t.CreatedAt)
                };

                // Paginação
                var totalCount = await _tagRepository.CountAsync(query, cancellationToken);
                var items = await _tagRepository.GetPagedAsync(
                    query, 
                    request.PageNumber, 
                    request.PageSize, 
                    cancellationToken);

                // Cria o resultado paginado
                var paginatedList = new PaginatedList<Tag>(
                    items.ToList(), 
                    totalCount, 
                    request.PageNumber, 
                    request.PageSize);

                return Result<PaginatedList<Tag>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<Tag>>.Error($"Erro ao buscar as tags: {ex.Message}");
            }
        }
    }
}
