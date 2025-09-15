using Ardalis.Result;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Categories
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;

        public DeleteCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ILogger<DeleteCategoryCommandHandler> logger
        )
        {
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(
            DeleteCategoryCommand request,
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
                    return Result<bool>.NotFound("Categoria não encontrada.");
                }

                // Verifica se a categoria tem subcategorias
                var hasSubCategories = await _categoryRepository.HasSubCategoriesAsync(
                    request.Id,
                    cancellationToken
                );

                if (hasSubCategories)
                {
                    return Result<bool>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                ErrorMessage =
                                    "Não é possível excluir uma categoria que possui subcategorias.",
                            },
                        }
                    );
                }

                // Verifica se a categoria está sendo usada por produtos
                var hasProducts = await _categoryRepository.HasProductsAsync(
                    request.Id,
                    cancellationToken
                );

                if (hasProducts)
                {
                    return Result<bool>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                ErrorMessage =
                                    "Não é possível excluir uma categoria que possui produtos associados.",
                            },
                        }
                    );
                }

                // Remove a categoria
                await _categoryRepository.DeleteAsync(category, cancellationToken);

                return Result<bool>.Success(true); // Retorna true para indicar sucesso
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {CategoryId}", request.Id);
                return Result<bool>.Error("Erro inesperado ao excluir a categoria");
            }
        }
    }
}
