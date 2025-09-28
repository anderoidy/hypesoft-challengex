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
                _logger.LogInformation("Iniciando criação de categoria: {Name}", request.Name);

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("Nome da categoria é obrigatório");
                    return Result.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                ErrorMessage = "O nome da categoria é obrigatório.",
                            },
                        }
                    );
                }

                var existing = await _categoryRepository.GetByNameAsync(
                    request.Name,
                    cancellationToken
                );
                if (existing != null)
                {
                    _logger.LogWarning("Categoria já existe: {Name}", request.Name);
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

                var category = new Category(
                    request.Name,
                    request.Description,
                    imageUrl: null,
                    isActive: true,
                    parentCategoryId: null,
                    slug: null
                );

                _logger.LogInformation(
                    "Categoria criada - ID antes da auditoria: {CategoryId}",
                    category.Id
                );

                // Auditoria
                category.SetCreatedBy(request.CreatedBy ?? "system");
                category.UpdateAuditFields(request.CreatedBy ?? "system");

                _logger.LogInformation("Categoria antes do save - ID: {CategoryId}", category.Id);

                var savedCategory = await _categoryRepository.AddAsync(category, cancellationToken);

                _logger.LogInformation(
                    "Categoria após save - ID salvo: {SavedCategoryId}",
                    savedCategory?.Id
                );
                _logger.LogInformation(
                    "Categoria após save - É null? {IsNull}",
                    savedCategory == null
                );

                if (savedCategory == null)
                {
                    _logger.LogError("Falha ao salvar categoria - savedCategory é null");
                    return Result.Error("Falha ao salvar a categoria");
                }

                if (savedCategory.Id == Guid.Empty)
                {
                    _logger.LogError(
                        "GUID vazio retornado do repositório para categoria: {Name}",
                        request.Name
                    );
                    return Result.Error(
                        "Erro interno: ID da categoria não foi gerado corretamente"
                    );
                }

                _logger.LogInformation(
                    "Categoria criada com sucesso - ID: {CategoryId}",
                    savedCategory.Id
                );

                // Retorna o ID da categoria salva no banco
                return Result.Success(savedCategory.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria {CategoryName}", request.Name);
                return Result.Error("Erro inesperado ao criar a categoria");
            }
        }
    }
}
