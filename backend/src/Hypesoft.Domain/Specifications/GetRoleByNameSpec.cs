using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class GetRoleByNameSpec : Specification<ApplicationRole>
    {
        public GetRoleByNameSpec(string roleName)
        {
            Query.Where(role => role.Name == roleName);
        }
    }
}
