using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Hypesoft.Domain.Entities;

[BsonIgnoreExtraElements]
public class IdentityRole : IEntity<string>
{
    public IdentityRole()
    {
        Id = Guid.NewGuid().ToString();
        ConcurrencyStamp = Guid.NewGuid().ToString();
        NormalizedName = string.Empty;
        Name = string.Empty;
        NormalizedName = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityRole"/> class with the specified role name.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    public IdentityRole(string roleName)
        : this()
    {
        Name = roleName ?? throw new ArgumentNullException(nameof(roleName));
        NormalizedName = roleName.ToUpperInvariant();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [BsonId]
    public string Id { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string ConcurrencyStamp { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public override string ToString()
    {
        return Name ?? string.Empty;
    }
}
