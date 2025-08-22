using System;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// Defines the interface for domain events.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        DateTimeOffset OccurredOn { get; }

        /// <summary>
        /// Gets the unique identifier for the event.
        /// </summary>
        Guid EventId { get; }
    }
}
