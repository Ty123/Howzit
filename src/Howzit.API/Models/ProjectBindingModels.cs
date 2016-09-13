using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class ProjectViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int ManagerId { get; set; }
        [Required]
        public int CustomerId { get; set; }
    }

    public class ChangeManagerModel
    {
        [Required]
        public int OldManagerId { get; set; }
        [Required]
        public int NewManagerId { get; set; }
    }
   
}