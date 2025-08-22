using System;
using System.Collections.Generic;
using Hypesoft.Application.Common.Mappings;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Application.Common.Dtos;

public class RoleDto : IMapFrom<ApplicationRole>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<string> Claims { get; set; } = new List<string>();
}
