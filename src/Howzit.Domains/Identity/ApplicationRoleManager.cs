using Howzit.Domains.Models;
using Howzit.Domains.Contracts;
using Microsoft.AspNet.Identity;


namespace Howzit.Domains.Models
{
    public class ApplicationRoleManager : RoleManager<ApplicationRole, int>
    {
        private readonly IRoleStore<ApplicationRole, int> _roleStore;
        public ApplicationRoleManager(IRoleStore<ApplicationRole, int> roleStore)
            : base(roleStore)
        {
            _roleStore = roleStore;
        }


        public ApplicationRole FindRoleByName(string roleName)
        {
            return this.FindByName(roleName); // RoleManagerExtensions
        }

        public IdentityResult CreateRole(ApplicationRole role)
        {
            return this.Create(role); // RoleManagerExtensions
        }
    }

}