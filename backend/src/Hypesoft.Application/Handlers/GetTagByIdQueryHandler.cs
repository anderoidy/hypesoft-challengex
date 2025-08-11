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
    public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, Result<Tag>>
    {
        private readonly ITagRepository _tagRepository;

        public GetTagByIdQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<Tag>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a tag pelo ID
                var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
                
                // Se não encontrar, retorna NotFound
                if (tag == null)
                {
                    return Result<Tag>.NotFound($"Tag com ID {request.Id} não encontrada.");
                }

                return Result<Tag>.Success(tag);
            }
            catch (Exception ex)
            {
                return Result<Tag>.Error($"Erro ao buscar a tag: {ex.Message}");
            }
        }
    }
}
