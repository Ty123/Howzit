using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class SupplierViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class SupplierContactModel
    {

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

    }
}