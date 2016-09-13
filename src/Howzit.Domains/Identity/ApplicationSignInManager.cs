using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Howzit.Domains.Models
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IAuthenticationManager _authenticationManager;

        public ApplicationSignInManager(ApplicationUserManager userManager,
                                        IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        {
            _userManager = userManager;
            _authenticationManager = authenticationManager;
        }
    }
}