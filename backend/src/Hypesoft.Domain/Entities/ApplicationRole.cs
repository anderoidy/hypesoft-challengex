using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Domain.Entities
{
    /// <summary>
    /// Represents a role in the application.
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole()
        {
            Claims = new List<IdentityRoleClaim<Guid>>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class with the specified role name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        public ApplicationRole(string roleName) : base(roleName)
        {
            Claims = new List<IdentityRoleClaim<Guid>>();
            CreatedAt = DateTime.UtcNow;
        }

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
        public DateTime? UpdatedAt { get;set; }

        /// <summary>
        /// Gets the date and time when the role was last modified.
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Gets the ID of the user who last modified the role.
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the claims associated with the role.
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<Guid>> Claims { get; set; }

        /// <summary>
        /// Sets the user who created the role.
        /// </summary>
        /// <param name="userId">The ID of the user who created the role.</param>
        public void SetCreatedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(userId));
                
            CreatedBy = userId;
        }

        /// <summary>
        /// Sets the date and time when the role was last updated.
        /// </summary>
        /// <param name="updatedAt">The date and time of the update.</param>
        public void SetUpdatedAt(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }

        /// <summary>
        /// Sets the user who last modified the role and updates the last modified timestamp.
        /// </summary>
        /// <param name="userId">The ID of the user who modified the role.</param>
        /// <param name="modifiedAt">Optional. The date and time of the modification. If not provided, the current UTC time will be used.</param>
        public void SetLastModifiedBy(string userId, DateTime? modifiedAt = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(userId));
                
            LastModifiedBy = userId;
            LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            UpdatedAt = LastModifiedAt;
        }
    }
}
