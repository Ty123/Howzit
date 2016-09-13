using Microsoft.AspNet.Identity;

namespace Howzit.Domains.Models
{
    public class ApplicationRoleStore 
    {
        private readonly IRoleStore<ApplicationRole, int> _roleStore;

        public ApplicationRoleStore(IRoleStore<ApplicationRole, int> roleStore)
        {
            _roleStore = roleStore;
        }
    }
}