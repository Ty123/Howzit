using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class CustomerViewModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string CompanyName { get; set; }
        
        [DataType(DataType.Url)]
        public String Website { get; set; }

    }

    public class CustomerContactModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public String Initial { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public String MiddleName { get; set; }

        [Required]
        public String Position { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool IsPrimary { get; set; }
    }
}