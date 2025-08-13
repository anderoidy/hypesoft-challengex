using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;

namespace Hypesoft.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// </summary>
    public abstract class BaseEntity : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the date and time when this entity was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when this entity was last modified.
        /// </summary>
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created this entity.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who last modified this entity.
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this entity was deleted.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who deleted this entity.
        /// </summary>
        public string? DeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the concurrency token for optimistic concurrency control.
        /// </summary>
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the domain events associated with this entity.
        /// </summary>
        private List<IDomainEvent>? _domainEvents;

        /// <summary>
        /// Gets the domain events associated with this entity.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>().AsReadOnly();
        /// <summary>
        /// Adds a domain event to this entity.
        /// </summary>
        /// <param name="eventItem">The domain event to add.</param>
        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents ??= new List<IDomainEvent>();
            _domainEvents.Add(eventItem);
        }

        /// <summary>
        /// Removes a domain event from this entity.
        /// </summary>
        /// <param name="eventItem">The domain event to remove.</param>
        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        /// <summary>
        /// Clears all domain events from this entity.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}
