using Microsoft.AspNet.Identity;

namespace Howzit.Domains.Models
{
    public class ApplicationUserStore 
    {
        private readonly IUserStore<ApplicationUser, int> _userStore;

        public ApplicationUserStore(IUserStore<ApplicationUser, int> userStore)
        {
            _userStore = userStore;
        }
    }
}