using Howzit.Domains.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Howzit.Domains.Models
{
    [Table("TaskLog")]
    public class TaskLog : Logging
    {
        public TaskLog(string message, String error, Log type, ApplicationUser actionLogger, Task row)
            : base(message, error, type, actionLogger)
        {
            Row = row == null ? 0 : row.Id;
        }

        #region Members

        public int Row { get; set; }

        #endregion
    }
}
