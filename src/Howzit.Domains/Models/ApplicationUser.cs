using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Howzit.Domains.Models
{
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationUser()
        {
            UserTasks = new HashSet<UserTask>();
            Comments = new HashSet<Comment>();
            Contacts = new HashSet<UserContact>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }

        public virtual Manager Manager { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual ICollection<UserContact> Contacts { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
