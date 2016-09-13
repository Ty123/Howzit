using Howzit.Domains.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;


namespace Howzit.Domains.Models
{
    public abstract class Logging
    {

        public Logging() { }

        public Logging(string message, String error, Log type, ApplicationUser actionLogger)
        {
            Message = message;
            Error = error;
            Type = type;
            Created = DateTime.Now;
            Logger = actionLogger == null ? 0 : actionLogger.Id;
        }

        #region members

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Message { get; set; }
        public String Error { get; set; }
        public Log Type { get; set; }
        public DateTime Created { get; set; }
        public int Logger { get; set; }

        #endregion

    }
}
