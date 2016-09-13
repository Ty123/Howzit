using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class Profile
    {
        public Profile() { }

        public Profile(string firstName, string lastName, String middleName, string initial, string gender, string phone, string address, ApplicationUser user)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            Gender = gender;
            Initial = initial;
            Phone = phone;
            Address = address;
            Created = DateTime.Now;
            CreateBy = user.Id; // creator
            IsActive = true;
            User = user;
        }

        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public String MiddleName { get; set; }
        public string Gender { get; set; }
        public string Initial { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public bool IsActive { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }

        public virtual ApplicationUser User { get; set; }

    }
}
