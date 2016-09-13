using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class Manager
    {
        public Manager() {
            Projects = new HashSet<Project>();
        }

        public Manager(ApplicationUser manager) : this()
        {
            User = manager;
        }

        [Key, ForeignKey("User")]
        public int Id { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
    }
}
