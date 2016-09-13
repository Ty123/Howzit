using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class TaskMaterial
    {
        public TaskMaterial()
        {
        }

        public TaskMaterial(Task task, Material material, decimal amount)
        {
            Task = task;
            Material = material;
            Amount = amount;
        }

        [Key]
        public int Id { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        [ForeignKey("Material")]
        public int MaterialId { get; set; }
        public decimal Amount { get; set; }
        public virtual Task Task { get; set; }
        public virtual Material Material { get; set; }

    }
}
