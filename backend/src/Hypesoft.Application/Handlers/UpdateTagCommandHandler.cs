using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers
{
    public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Result<Tag>>
    {
        private readonly ITagRepository _tagRepository;

        public UpdateTagCommandHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<Tag>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a tag existente
                var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
                if (tag == null)
                {
                    return Result<Tag>.NotFound($"Tag com ID {request.Id} não encontrada.");
                }

                // Verifica se já existe outra tag com o mesmo nome
                if (!string.Equals(tag.Name, request.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
                    if (existingTag != null && existingTag.Id != request.Id)
                    {
                        return Result<Tag>.Error($"Já existe outra tag com o nome '{request.Name}'.");
                    }
                }

                // Atualiza os campos da tag
                tag.Update(
                    request.Name,
                    request.Description,
                    request.Icon,
                    request.Color,
                    request.IsActive ?? tag.IsActive,
                    request.DisplayOrder ?? tag.DisplayOrder,
                    request.UserId
                );

                // Atualiza a tag no repositório
                _tagRepository.Update(tag);
                await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Tag>.Success(tag);
            }
            catch (Exception ex)
            {
                return Result<Tag>.Error($"Erro ao atualizar a tag: {ex.Message}");
            }
        }
    }
}
