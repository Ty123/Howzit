using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class TaskTool
    {
        public TaskTool() { }

        public TaskTool(Task task, Tool tool, DateTime from, DateTime to)
        {
            Task = task;
            Tool = tool;
            DateFrom = from;
            DateTo = to;
        }

        [Key, Column(Order = 0)]
        public int TaskId { get; set; }
        [Key, Column(Order = 1)]
        public int ToolId { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public virtual Task Task { get; set; }
        public virtual Tool Tool { get; set; }
    }
}
