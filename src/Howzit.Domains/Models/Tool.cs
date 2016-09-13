using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Howzit.Domains.Models
{
    public class Tool
    {
        public Tool()
        {
            this.TaskTools = new HashSet<TaskTool>();
            Purchased = DateTime.Now;
            EstimateDecommision = DateTime.Now;
        }

        public Tool(string name, DateTime purchased, DateTime decomission, DateTime actualDecomission, decimal cost, int maintainceId, int supplierId, ApplicationUser actionLogger)
            : this()
        {
            Name = name;
            Purchased = purchased;
            EstimateDecommision = decomission;
            ActualDecommision = actualDecomission;
            Cost = cost;
            MaintenanceId = maintainceId;
            SupplierId = supplierId;
            CreateBy = actionLogger.Id;
            Created = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Purchased { get; set; }
        public DateTime EstimateDecommision { get; set; }
        public Nullable<DateTime> ActualDecommision { get; set; }
        public decimal Cost { get; set; }
        public Nullable<int> MaintenanceId { get; set; }
        public int SupplierId { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int CreateBy { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }
        
        public virtual ICollection<TaskTool> TaskTools { get; set; }
    }
}
