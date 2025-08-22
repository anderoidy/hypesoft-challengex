using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Hypesoft.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class ApplicationRole : IdentityRole, IEntity<string>
    {
        public ApplicationRole()
            : base()
        {
            Id = Guid.NewGuid().ToString();
            ConcurrencyStamp = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }

        public ApplicationRole(string roleName)
            : base(roleName)
        {
            NormalizedName = roleName?.ToUpperInvariant();
        }

        [BsonId]
        public new string Id { get; set; }

        public new string Name { get; set; } = string.Empty;

        public new string NormalizedName { get; set; } = string.Empty;

        public new string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemRole { get; set; }

        [BsonIgnore]
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } =
            new List<IdentityUserRole<string>>();

        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; } =
            new List<IdentityRoleClaim<string>>();

        public void Update(string name, string description, string updatedBy)
        {
            Name = name;
            NormalizedName = name?.ToUpperInvariant();
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        public void Deactivate(string updatedBy)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        public void SetLastModifiedBy(string modifiedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = modifiedBy;
        }
    }
}
