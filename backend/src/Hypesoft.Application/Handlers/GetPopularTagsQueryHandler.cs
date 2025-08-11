using System;
using System.Collections.Generic;
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
    public class GetPopularTagsQueryHandler : IRequestHandler<GetPopularTagsQuery, Result<IEnumerable<Tag>>>
    {
        private readonly ITagRepository _tagRepository;

        public GetPopularTagsQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<IEnumerable<Tag>>> Handle(GetPopularTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validação do parâmetro de entrada
                if (request.Count <= 0)
                {
                    return Result<IEnumerable<Tag>>.Error("A contagem de tags deve ser maior que zero.");
                }

                // Limita o número máximo de tags a serem retornadas
                var maxCount = Math.Min(request.Count, 100); // Limite máximo de 100 tags

                // Consulta as tags mais populares
                var query = _tagRepository.GetAll();
                
                // Filtra apenas tags ativas, se necessário
                if (request.OnlyActive)
                {
                    query = query.Where(t => t.IsActive);
                }

                // Ordena por contagem de uso (mais usadas primeiro) e depois por nome
                var popularTags = await _tagRepository.ListAsync(
                    query
                        .OrderByDescending(t => t.UsageCount)
                        .ThenBy(t => t.Name)
                        .Take(maxCount),
                    cancellationToken);

                return Result<IEnumerable<Tag>>.Success(popularTags);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<Tag>>.Error($"Erro ao buscar as tags populares: {ex.Message}");
            }
        }
    }
}
