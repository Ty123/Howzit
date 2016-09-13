using Howzit.Domains.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class TaskViewModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Summary { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public State State { get; set; }
       
        [Required]
        public decimal Progress { get; set; }
  
        [Required]
        public int Priority { get; set; }
 
        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public List<int> AssigneeIds { get; set; }
    }

    public class TaskAssignModel
    {
        [Required]
        public int UserId { get; set; }
    }

    public class ChangeTaskOwnerModel
    {
        [Required]
        public int OldOwnerId { get; set; }
        
        [Required]
        public int NewOwnerId { get; set; }
    }

    public class ChangeTaskAssigneeModel
    {        
        [Required]
        public int OldUserId { get; set; }
        
        [Required]
        public int NewUserId { get; set; }
    }
}