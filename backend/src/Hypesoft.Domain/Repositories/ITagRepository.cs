using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default);
}
