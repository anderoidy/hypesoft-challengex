using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers
{
    public class GetTagBySlugQueryHandler : IRequestHandler<GetTagBySlugQuery, Result<Tag>>
    {
        private readonly ITagRepository _tagRepository;

        public GetTagBySlugQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<Tag>> Handle(GetTagBySlugQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validação do parâmetro de entrada
                if (string.IsNullOrWhiteSpace(request.Slug))
                {
                    return Result<Tag>.Error("O slug da tag não pode ser vazio.");
                }

                // Busca a tag pelo slug
                var tag = await _tagRepository.GetBySlugAsync(request.Slug.Trim(), cancellationToken);
                
                // Se não encontrar, retorna NotFound
                if (tag == null)
                {
                    return Result<Tag>.NotFound($"Tag com o slug '{request.Slug}' não encontrada.");
                }

                return Result<Tag>.Success(tag);
            }
            catch (Exception ex)
            {
                return Result<Tag>.Error($"Erro ao buscar a tag pelo slug: {ex.Message}");
            }
        }
    }
}
