using Ardalis.Result;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Categories
{
    public class UpdateCategoryCommandHandler
        : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;

        public UpdateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ILogger<UpdateCategoryCommandHandler> logger
        )
        {
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<CategoryDto>> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // Busca a categoria existente
                var category = await _categoryRepository.GetByIdAsync(
                    request.Id,
                    cancellationToken
                );

                if (category == null)
                {
                    return Result.NotFound("Categoria não encontrada.");
                }

                // Verifica se já existe outra categoria com o mesmo nome (exceto a atual)
                var existing = await _categoryRepository.GetByNameAsync(
                    request.Name,
                    cancellationToken
                );

                if (existing != null && existing.Id != request.Id)
                {
                    return Result.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                ErrorMessage = "Já existe uma categoria com esse nome.",
                            },
                        }
                    );
                }

                // Atualiza os dados da categoria usando o método Update da entidade
                category.Update(
                    name: request.Name,
                    description: request.Description,
                    imageUrl: null, // Mantém a imagem atual
                    userId: request.ModifiedBy,
                    slug: null // Mantém o slug atual
                );

                // Atualiza IsMainCategory baseado no IsActive do request
                category.SetAsMainCategory(request.IsActive, request.ModifiedBy);

                // Salva as alterações
                var updated = await _categoryRepository.UpdateAsync(category, cancellationToken);

                // Converte para DTO
                var categoryDto = new CategoryDto(
                    Id: updated.Id,
                    Name: updated.Name,
                    Description: updated.Description,
                    ImageUrl: updated.ImageUrl,
                    IsMainCategory: updated.IsMainCategory,
                    ParentCategoryId: updated.ParentCategoryId,
                    ParentCategoryName: updated.ParentCategory?.Name
                );

                return Result.Success(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {CategoryId}", request.Id);
                return Result.Error("Erro inesperado ao atualizar a categoria");
            }
        }
    }
}
