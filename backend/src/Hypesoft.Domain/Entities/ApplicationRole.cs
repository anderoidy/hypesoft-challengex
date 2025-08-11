using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Domain.Entities
{
    /// <summary>
    /// Represents a role in the application.
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class with the specified role name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        /// <summary>
        /// Gets or sets the description for the role.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the role was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the role was last modified.
        /// </summary>
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the role.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who last modified the role.
        /// </summary>
        public string? ModifiedBy { get; set; }
    }
}
