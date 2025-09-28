using System;

namespace Hypesoft.Domain.Common.Interfaces
{
    public interface IAuditableEntity
    {
        string? CreatedBy { get; }
        DateTimeOffset CreatedAt { get; }
        string? ModifiedBy { get; }
        DateTimeOffset? ModifiedAt { get; }
    }
}
