using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Howzit.Domains.Enums;

namespace Howzit.Domains.Models
{
    public class Task
    {
        public Task()
        {
            this.Comments = new HashSet<Comment>();
            this.UserTasks = new HashSet<UserTask>();
            this.TaskMaterials = new HashSet<TaskMaterial>();
        }

        public Task(string summary, string description, State state, decimal progress, int priority, Project project, Owner owner, DateTime start, DateTime end, ApplicationUser actionLogger)
            : this()
        {
            Summary = summary;
            Description = description;
            State = state;
            Progress = progress;
            Priority = priority;
            Created = DateTime.Now;
            CreateBy = actionLogger.Id;
            Project = project;
            Owner = owner;
            DateStart = start;
            DateEnd = end;
        }

        public int Id { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public State State { get; set; }
        public decimal Progress { get; set; }
        public int Priority { get; set; }
        public DateTime Created { get; set; }
        public int CreateBy { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }   
        
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        [ForeignKey("Owner")]
        public int OwnerId { get; set; }
        public virtual Owner Owner { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<TaskMaterial> TaskMaterials { get; set; }
    }
}
