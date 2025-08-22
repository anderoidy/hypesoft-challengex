using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace Hypesoft.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser : IEntity
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

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

        // ApplicationUser specific properties
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }
        public bool IsDeleted { get; private set; }

        // Navigation properties
        [BsonElement("UserRoles")]
        public ICollection<string> RoleIds { get; set; } = new List<string>();

        [BsonIgnore]
        public ICollection<ApplicationUserRole> UserRoles { get; set; } =
            new List<ApplicationUserRole>();

        // Audit methods
        public void SetUpdatedAt(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }

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
            SetLastModifiedBy(deletedBy);
        }
    }
}
