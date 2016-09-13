using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class UserContact : Contact
    {
        public UserContact() { }

        public UserContact(string firstName, string lastName, String middleName, string initial, string phone, string email, bool isPrimary, string relationship, ApplicationUser user, ApplicationUser logger)
            : base(firstName,lastName,middleName,initial,isPrimary,phone,email,logger)
        {
            User = user;
            Relationship = relationship;
        }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Relationship { get; set; }
    }
}
