using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using Hypesoft.Domain.Common;

namespace Hypesoft.Domain.Entities
{
    /// <summary>
    /// Represents a role in the application.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ApplicationRole : IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole()
        {
            Id = Guid.NewGuid();
            ConcurrencyStamp = Guid.NewGuid().ToString();
            NormalizedName = Name?.ToUpperInvariant();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class with the specified role name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        public ApplicationRole(string roleName)
            : this()
        {
            Name = roleName;
            NormalizedName = roleName?.ToUpperInvariant();
        }

        /// <summary>
        /// Gets or sets the primary key for this role.
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        public string? NormalizedName { get; set; }

        /// <summary>
        /// A random value that should change whenever a role is persisted to the store.
        /// </summary>
        public string? ConcurrencyStamp { get; set; }

        /// <summary>
        /// Gets or sets the description for the role.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets the date and time when the role was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets the ID of the user who created the role.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets the date and time when the role was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who last updated the role.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the role is system reserved.
        /// </summary>
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// Navigation property for the users in this role.
        /// </summary>
        [BsonIgnore]
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } =
            new List<ApplicationUserRole>();

        /// <summary>
        /// Updates the role with the specified values.
        /// </summary>
        /// <param name="name">The role name.</param>
        /// <param name="description">The role description.</param>
        /// <param name="updatedBy">The ID of the user updating the role.</param>
        public void Update(string name, string? description, string updatedBy)
        {
            Name = name;
            NormalizedName = name?.ToUpperInvariant();
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Marks the role as inactive.
        /// </summary>
        /// <param name="updatedBy">The ID of the user performing the action.</param>
        public void Deactivate(string updatedBy)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sets the user who last modified this role.
        /// </summary>
        /// <param name="modifiedBy">The username of the user who made the modification.</param>
        public void SetLastModifiedBy(string modifiedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = modifiedBy;
        }
    }
}
