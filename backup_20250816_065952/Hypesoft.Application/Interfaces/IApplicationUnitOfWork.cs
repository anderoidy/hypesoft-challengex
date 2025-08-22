using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hypesoft.Application.Common.Interfaces;

public interface IApplicationUnitOfWork : IUnitOfWork
{
    // Mantendo apenas os membros específicos de IApplicationUnitOfWork
    // Os métodos de IUnitOfWork já são herdados

    IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : BaseEntity;

    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }

    // Métodos específicos de negócio que envolvem múltiplos repositórios
    Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default);
}
