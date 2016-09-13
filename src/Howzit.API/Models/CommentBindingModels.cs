using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class CommentViewModel
    {
        [Key]
        public int Id { get; set; }
       
        [Required]
        public string Message { get; set; }
       
        [Required]
        public int TaskId { get; set; }
       
        [Required]
        public int UserId { get; set; }
    }
}