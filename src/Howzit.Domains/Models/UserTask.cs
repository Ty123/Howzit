using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Howzit.Domains.Models
{
    [Table("UserTask")]
    public class UserTask
    {
        public UserTask() { }

        public UserTask(ApplicationUser user)
        {
            User = user;
        }

        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Task Task { get; set; }
    }
}
