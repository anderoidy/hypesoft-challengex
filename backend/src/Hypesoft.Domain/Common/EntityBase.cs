using System;

namespace Hypesoft.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    public void Deactivate(string userId)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void Activate(string userId)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void UpdateAuditFields(string userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void SetCreatedBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        CreatedBy = userId;
        UpdatedBy = userId;
    }
    
    public void SetLastModifiedBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}
