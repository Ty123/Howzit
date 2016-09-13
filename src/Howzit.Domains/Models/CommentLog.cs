using Howzit.Domains.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class CommentLog : Logging
    {
        public CommentLog(string message, String error, Log type, ApplicationUser actionLogger, Comment row) : base(message, error, type, actionLogger)
        {
            Row = row == null? 0 : row.Id;
        }

        #region Members 

        public int Row { get; set; }

        #endregion
    }
}
