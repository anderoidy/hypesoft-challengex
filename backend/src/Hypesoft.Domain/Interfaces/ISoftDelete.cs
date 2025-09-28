using System;
using Hypesoft.Domain.Common.Interfaces;

namespace Hypesoft.Domain.Common.Interfaces
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }

        DateTimeOffset? DeletedAt { get; set; }

        string? DeletedBy { get; set; }
    }
}
