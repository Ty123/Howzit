using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class Owner
    {
        public Owner()
        {

        }

        public Owner(ApplicationUser owner)
        {
            User = owner;
        }

        [Key, ForeignKey("User")]
        public int OwnerId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
