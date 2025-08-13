using System;
using Hypesoft.Domain.Common.Interfaces;

namespace Hypesoft.Domain.Common;

public abstract class EntityBase : BaseEntity
{
    public bool IsActive { get; protected set; } = true;

    public void Deactivate(string userId)
    {
        IsActive = false;
        UpdateAuditFields(userId);
    }

    public void Activate(string userId)
    {
        IsActive = true;
        UpdateAuditFields(userId);
    }

    public void UpdateAuditFields(string userId)
    {
        ModifiedAt = DateTimeOffset.UtcNow;
        ModifiedBy = userId;
    }

    public void SetCreatedBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        CreatedBy = userId;
        ModifiedBy = userId;
    }
    
    public void SetLastModifiedBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        ModifiedAt = DateTimeOffset.UtcNow;
        ModifiedBy = userId;
    }
}
