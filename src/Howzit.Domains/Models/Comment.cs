using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Howzit.Domains.Models
{
    public class Comment
    {
        public Comment() {
            IsApproved = false;
        }

        public Comment(string message, Howzit.Domains.Models.Task task, ApplicationUser user): this()
        {
            Message = message;
            Task = task;
            Created = DateTime.Now;
            User = user;
        }

        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public bool IsApproved { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Task Task { get; set; }
        public int? UpdateBy { get; set; }
        public Nullable<DateTime> Updated { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
