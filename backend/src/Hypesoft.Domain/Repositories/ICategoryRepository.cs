using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default);
}
