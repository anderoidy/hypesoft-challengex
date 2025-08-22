using MongoDB.Bson.Serialization.Attributes;

namespace Hypesoft.Domain.Entities;

[BsonIgnoreExtraElements]
public class ApplicationUserRole
{
    [BsonElement("userId")]
    public Guid UserId { get; set; }

    [BsonElement("roleId")]
    public Guid RoleId { get; set; }

    [BsonElement("assignedAt")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("assignedBy")]
    public string? AssignedBy { get; set; }

    [BsonIgnore]
    public virtual ApplicationUser? User { get; set; }

    [BsonIgnore]
    public virtual ApplicationRole? Role { get; set; }

    public ApplicationUserRole() { }

    public ApplicationUserRole(Guid userId, Guid roleId, string? assignedBy = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
    }
}
