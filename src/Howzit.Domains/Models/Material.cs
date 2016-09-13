using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Howzit.Domains.Models
{
    public class Material
    {
        public Material()
        {
            this.TaskMaterials = new HashSet<TaskMaterial>();
        }

        public Material(string name, string description, decimal quantity, Supplier supplier, ApplicationUser actionLogger)
            : this()
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            Supplier = supplier;
            Created = DateTime.Now;
            CreateBy = actionLogger.Id;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int CreateBy { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<TaskMaterial> TaskMaterials { get; set; }

    }
}
