using Ardalis.Result;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Categories
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ILogger<CreateCategoryCommandHandler> logger
        )
        {
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // Verifica se já existe categoria com esse nome
                var existing = await _categoryRepository.GetByNameAsync(
                    request.Name,
                    cancellationToken
                );
                if (existing != null)
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

                // Cria nova categoria
                var category = new Category(
                    request.Name,
                    request.Description,
                    imageUrl: null,
                    isMainCategory: true,
                    parentCategoryId: null,
                    slug: null
                );

                if (!string.IsNullOrEmpty(request.CreatedBy))
                {
                    category.UpdateAuditFields(request.CreatedBy);
                }

                // Salva no repositório
                var created = await _categoryRepository.AddAsync(category, cancellationToken);

                // Retorna o ID da categoria criada
                return Result.Success(created.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria {CategoryName}", request.Name);
                return Result.Error("Erro inesperado ao criar a categoria");
            }
        }
    }
}
