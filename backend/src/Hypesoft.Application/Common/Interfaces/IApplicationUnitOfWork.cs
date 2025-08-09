using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Common.Interfaces;

public interface IApplicationUnitOfWork : IUnitOfWork
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    
    // Adicionar outros repositórios específicos do domínio aqui quando necessário
    
    // Métodos específicos de negócio que envolvem múltiplos repositórios
    Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> IsTagInUseAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalTagCountAsync(CancellationToken cancellationToken = default);
}
