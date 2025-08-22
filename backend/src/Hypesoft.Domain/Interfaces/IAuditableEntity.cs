using System;

namespace Hypesoft.Domain.Common.Interfaces;

public interface IAuditableEntity
{
    string? CreatedBy { get; set; }

    DateTimeOffset CreatedAt { get; set; }

    string? ModifiedBy { get; set; }

    DateTimeOffset? ModifiedAt { get; set; }
}
