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
    public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Tag>>
    {
        private readonly ITagRepository _tagRepository;

        public CreateTagCommandHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result<Tag>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verifica se já existe uma tag com o mesmo nome
                var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
                if (existingTag != null)
                {
                    return Result<Tag>.Error($"Já existe uma tag com o nome '{request.Name}'.");
                }

                // Cria uma nova tag
                var tag = new Tag(
                    request.Name,
                    request.Description,
                    request.Icon,
                    request.Color,
                    request.IsActive,
                    request.DisplayOrder,
                    request.UserId
                );

                // Salva a tag no repositório
                await _tagRepository.AddAsync(tag, cancellationToken);
                await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Tag>.Success(tag);
            }
            catch (Exception ex)
            {
                return Result<Tag>.Error($"Erro ao criar a tag: {ex.Message}");
            }
        }
    }
}
