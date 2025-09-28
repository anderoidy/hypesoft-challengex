using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Category?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation("Buscando categoria por ID: {CategoryId}", id);
                var category = await _context.Categories.FindAsync(
                    new object[] { id },
                    cancellationToken
                );

                if (category != null)
                {
                    _logger.LogInformation("Categoria encontrada: {CategoryName}", category.Name);
                }
                else
                {
                    _logger.LogWarning("Categoria não encontrada com ID: {CategoryId}", id);
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao buscar categoria por ID: {CategoryId}. Erro: {ErrorMessage}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<Category?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation("Buscando categoria por nome: {CategoryName}", name);
                var category = await _context.Categories.FirstOrDefaultAsync(
                    c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase),
                    cancellationToken
                );

                if (category != null)
                {
                    _logger.LogInformation("Categoria encontrada: {CategoryName}", category.Name);
                }
                else
                {
                    _logger.LogWarning("Categoria não encontrada com nome: {CategoryName}", name);
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao buscar categoria por nome: {CategoryName}. Erro: {ErrorMessage}",
                    name,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation("Buscando todas as categorias");
                var categories = await _context
                    .Categories.OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Encontradas {Count} categorias", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao buscar todas as categorias. Erro: {ErrorMessage}",
                    ex.Message
                );
                throw;
            }
        }

        public async Task<Category> AddAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Adicionando nova categoria: {CategoryName}",
                    category?.Name
                );

                if (category == null)
                {
                    _logger.LogError("Categoria não pode ser nula");
                    throw new ArgumentNullException(nameof(category));
                }

                if (category.Id == Guid.Empty)
                {
                    _logger.LogWarning("ID da categoria está vazio, gerando novo ID");
                    category.EnsureId();
                }

                _logger.LogInformation(
                    "ID da categoria antes de salvar: {CategoryId}",
                    category.Id
                );

                await _context.Categories.AddAsync(category, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ID da categoria após salvar: {CategoryId}", category.Id);

                if (category.Id == Guid.Empty)
                {
                    _logger.LogError("ID da categoria ainda está vazio após salvar!");
                    throw new InvalidOperationException(
                        "ID da categoria não foi gerado corretamente"
                    );
                }

                _logger.LogInformation(
                    "Categoria adicionada com sucesso. ID: {CategoryId}, Nome: {CategoryName}",
                    category.Id,
                    category.Name
                );
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao adicionar categoria: {CategoryName}. Erro: {ErrorMessage}",
                    category?.Name,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<Category> UpdateAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Atualizando categoria: {CategoryId}, Nome: {CategoryName}",
                    category.Id,
                    category.Name
                );

                if (category == null)
                {
                    _logger.LogError("Categoria não pode ser nula");
                    throw new ArgumentNullException(nameof(category));
                }

                var existingCategory = await GetByIdAsync(category.Id, cancellationToken);
                if (existingCategory == null)
                {
                    _logger.LogWarning(
                        "Categoria não encontrada para atualização: {CategoryId}",
                        category.Id
                    );
                    throw new KeyNotFoundException(
                        $"Categoria com ID {category.Id} não encontrada"
                    );
                }

                _context.Entry(existingCategory).CurrentValues.SetValues(category);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Categoria atualizada com sucesso: {CategoryId}",
                    category.Id
                );
                return await GetByIdAsync(category.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao atualizar categoria: {CategoryId}. Erro: {ErrorMessage}",
                    category?.Id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deletando categoria por ID: {CategoryId}", id);

                var category = await GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    _logger.LogWarning("Categoria não encontrada para deleção: {CategoryId}", id);
                    return false;
                }

                await DeleteAsync(category, cancellationToken);
                _logger.LogInformation("Categoria deletada com sucesso: {CategoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao deletar categoria por ID: {CategoryId}. Erro: {ErrorMessage}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task DeleteAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Deletando categoria: {CategoryId}, Nome: {CategoryName}",
                    category.Id,
                    category.Name
                );

                if (category == null)
                {
                    _logger.LogError("Categoria não pode ser nula");
                    throw new ArgumentNullException(nameof(category));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Categoria deletada com sucesso: {CategoryId}", category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao deletar categoria: {CategoryId}. Erro: {ErrorMessage}",
                    category?.Id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Verificando se categoria existe: {CategoryId}", id);
                var exists = await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
                _logger.LogInformation("Categoria {CategoryId} existe: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar existência da categoria: {CategoryId}. Erro: {ErrorMessage}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<bool> HasSubCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Verificando se categoria tem subcategorias: {CategoryId}",
                    categoryId
                );
                var hasSubCategories = await _context.Categories.AnyAsync(
                    c => c.ParentCategoryId == categoryId,
                    cancellationToken
                );
                _logger.LogInformation(
                    "Categoria {CategoryId} tem subcategorias: {HasSubCategories}",
                    categoryId,
                    hasSubCategories
                );
                return hasSubCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar subcategorias da categoria: {CategoryId}. Erro: {ErrorMessage}",
                    categoryId,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<bool> HasProductsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    "Verificando se categoria tem produtos: {CategoryId}",
                    categoryId
                );
                var hasProducts = await _context.Products.AnyAsync(
                    p => p.CategoryId == categoryId,
                    cancellationToken
                );
                _logger.LogInformation(
                    "Categoria {CategoryId} tem produtos: {HasProducts}",
                    categoryId,
                    hasProducts
                );
                return hasProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar produtos da categoria: {CategoryId}. Erro: {ErrorMessage}",
                    categoryId,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetMainCategoriesAsync(
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation("Buscando categorias principais");
                var mainCategories = await _context
                    .Categories.Where(c => c.ParentCategoryId == null)
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);
                _logger.LogInformation(
                    "Encontradas {Count} categorias principais",
                    mainCategories.Count
                );
                return mainCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao buscar categorias principais. Erro: {ErrorMessage}",
                    ex.Message
                );
                throw;
            }
        }
    }
}
