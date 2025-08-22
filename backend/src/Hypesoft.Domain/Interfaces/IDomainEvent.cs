using System;

namespace Hypesoft.Domain.Common.Interfaces
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }

        Guid EventId { get; }
    }
}
