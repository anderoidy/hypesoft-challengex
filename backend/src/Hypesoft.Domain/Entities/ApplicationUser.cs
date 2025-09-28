using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace Hypesoft.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser : IEntity<Guid>
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Identity-like props
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        // Custom
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }
        public bool IsDeleted { get; private set; }

        // Roles simples em Mongo
        [BsonElement("UserRoles")]
        public ICollection<string> RoleIds { get; set; } = new List<string>();

        [BsonIgnore]
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

        // Computed
        [BsonIgnore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Audit helpers
        public void SetCreated(string createdBy)
        {
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            LastModifiedBy = CreatedBy;
        }

        public void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

        public void SetLastModifiedBy(string userId, DateTime? modifiedAt = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(userId));

            LastModifiedBy = userId;
            UpdatedAt = modifiedAt ?? DateTime.UtcNow;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            SetLastModifiedBy(string.IsNullOrWhiteSpace(deletedBy) ? "system" : deletedBy);
        }

        public void SetLastLogin(DateTime loginTime)
        {
            LastLoginAt = loginTime;
            SetUpdatedAt(loginTime);
        }

        public void ActivateUser(string activatedBy)
        {
            IsActive = true;
            SetLastModifiedBy(string.IsNullOrWhiteSpace(activatedBy) ? "system" : activatedBy);
        }

        public void DeactivateUser(string deactivatedBy)
        {
            IsActive = false;
            SetLastModifiedBy(string.IsNullOrWhiteSpace(deactivatedBy) ? "system" : deactivatedBy);
        }
    }
}
