using System;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// Defines the interface for entities that support soft delete.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity is deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was deleted.
        /// </summary>
        DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who deleted the entity.
        /// </summary>
        string? DeletedBy { get; set; }
    }
}
