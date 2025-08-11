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
    public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Result>
    {
        private readonly ITagRepository _tagRepository;

        public DeleteTagCommandHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a tag existente
                var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
                if (tag == null)
                {
                    return Result.NotFound($"Tag com ID {request.Id} não encontrada.");
                }

                // Verifica se a tag está sendo usada em algum produto
                var isTagInUse = await _tagRepository.IsTagInUseAsync(request.Id, cancellationToken);
                if (isTagInUse)
                {
                    return Result.Error("Não é possível excluir esta tag pois ela está sendo usada em um ou mais produtos.");
                }

                // Remove a tag do repositório
                _tagRepository.Delete(tag);
                await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error($"Erro ao excluir a tag: {ex.Message}");
            }
        }
    }
}
