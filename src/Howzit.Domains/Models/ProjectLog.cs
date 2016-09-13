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
    public class ProjectLog : Logging
    {
        public ProjectLog(string message, String error, Log type, ApplicationUser actionLogger, Project row)
            : base(message, error, type, actionLogger)
        {
            Row = row == null ? 0 : row.Id;
        }

        #region Members 

        [Column("Row")]
        public int Row { get; set; }

        #endregion

    }
}
