using System;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// A marker interface for aggregate root entities.
    /// An aggregate root is an entity that is the root of an aggregate.
    /// It's the only member of the aggregate that outside objects are allowed to hold references to.
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        // Marker interface - no members required
    }
}
