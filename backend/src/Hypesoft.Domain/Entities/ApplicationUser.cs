using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IEntity
    {
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
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } =
            new List<ApplicationUserRole>();
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; } =
            new List<IdentityUserClaim<Guid>>();
        public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; set; } =
            new List<IdentityUserLogin<Guid>>();
        public virtual ICollection<IdentityUserToken<Guid>> Tokens { get; set; } =
            new List<IdentityUserToken<Guid>>();

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
