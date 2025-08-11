using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; } = new List<IdentityUserClaim<Guid>>();
        public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; set; } = new List<IdentityUserLogin<Guid>>();
        public virtual ICollection<IdentityUserToken<Guid>> Tokens { get; set; } = new List<IdentityUserToken<Guid>>();
    }
}
