using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Categories
{
    public class GetAllCategoriesQueryHandler
        : IRequestHandler<GetAllCategoriesQuery, Result<PaginatedList<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCategoriesQueryHandler> _logger;

        public GetAllCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<GetAllCategoriesQueryHandler> logger
        )
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PaginatedList<CategoryDto>>> Handle(
            GetAllCategoriesQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Processando GetAllCategoriesQuery");

                // Buscar todas as categorias
                var allCategories = await _categoryRepository.GetAllAsync();

                // Converter para lista para trabalhar mais facilmente
                var categoriesList = allCategories.ToList();

                _logger.LogInformation("Total de categorias encontradas: " + categoriesList.Count);

                // Para simplicidade, vamos retornar todas sem filtro por enquanto
                var categoryDtos = _mapper.Map<List<CategoryDto>>(categoriesList);

                // Criar resultado paginado simples
                var result = new PaginatedList<CategoryDto>(
                    categoryDtos,
                    categoriesList.Count,
                    1,
                    categoriesList.Count
                );

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar GetAllCategoriesQuery");
                return Result.Error("Erro interno: " + ex.Message);
            }
        }
    }
}
