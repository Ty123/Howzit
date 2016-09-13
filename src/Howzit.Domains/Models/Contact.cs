using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class Contact
    {
        public Contact() { }

        public Contact(string firstName, string lastName, String middleName, string initial, bool isPrimary, string phone, string email, ApplicationUser actionLogger)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            Initial = initial;           
            Phone = phone;
            Created = DateTime.Now;
            CreateBy = actionLogger.Id;
            Email = email;
            IsPrimary = isPrimary;
            IsActive = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public String MiddleName { get; set; }
        public string Initial { get; set; }
        public string Phone { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int CreateBy { get; set; }
        public int? UpdateBy { get; set; }

        public string Email { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
