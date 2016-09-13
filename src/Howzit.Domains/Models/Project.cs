using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Howzit.Domains.Enums;

namespace Howzit.Domains.Models
{
    public class Project
    {
        public Project()
        {
            this.Tasks = new HashSet<Task>();
        }

        public Project(string name, string description, Manager manager, Customer customer, State state, ApplicationUser user)
            : this()
        {
            Name = name;
            Description = description;
            Customer = customer;
            State = state;
            Created = DateTime.Now;
            CreateBy = user.Id;
            Manager = manager;
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public State State { get; set; }
        public DateTime Created { get; set; }
        public int CreateBy { get; set; }
        [ForeignKey("Manager")]
        public int ManagerId { get; set; }
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public DateTime? Updated { get; set; }
        public int? UpdateBy { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Manager Manager { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

    }
}
