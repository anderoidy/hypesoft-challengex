using Ardalis.Result;
using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Categories
{
    /// <summary>
    /// Handler para processar a query GetCategoryByIdQuery
    /// </summary>
    public class GetCategoryByIdQueryHandler 
        : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

        public GetCategoryByIdQueryHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<GetCategoryByIdQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CategoryDto>> Handle(
            GetCategoryByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"🔍 Buscando categoria por ID: {request.Id}");

                // Buscar categoria por ID
                var category = await _categoryRepository.GetByIdAsync(request.Id);

                if (category == null)
                {
                    _logger.LogWarning($"⚠️ Categoria não encontrada: {request.Id}");
                    return Result.NotFound($"Categoria com ID {request.Id} não foi encontrada");
                }

                _logger.LogInformation($"✅ Categoria encontrada: {category.Name}");

                // Mapear para DTO
                var categoryDto = _mapper.Map<CategoryDto>(category);

                return Result.Success(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erro ao buscar categoria por ID: {request.Id}");
                return Result.Error($"Erro interno: {ex.Message}");
            }
        }
    }
}
