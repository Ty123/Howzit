using Howzit.Domains.Enums;
using Howzit.Domains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Howzit.DAL.Services
{
    public class UserActionLog
    {
        #region Methods

        // log invalid model states
        public static UserLog LogModelStateError(ApiController controller, ApplicationUser actionLogger, ApplicationUser user, out string errors)
        {
            var modelErrors = controller.ModelState.Select(x => x.Value.Errors).Where(y => y.Count() > 0).FirstOrDefault();
            string mErrors = "";

            foreach (var error in modelErrors)
            {
                mErrors = string.Concat(mErrors, error.ErrorMessage + " | ");
            }

            errors = mErrors;

            return new UserLog("ModelState Errors!", mErrors, Log.ERROR, actionLogger, user);
        }

        // log exception details
        public static UserLog LogExceptionDetails(Exception ex, ApplicationUser actionLogger)
        {
            return new UserLog("Exception thrown!", ex.Message, Log.EXCEPTION, actionLogger, null);
        }

        // log update a record
        public static UserLog LogUpdateDetails(string message, ApplicationUser actionLogger, ApplicationUser user)
        {
            return new UserLog(message, null, Log.UPDATE, actionLogger, user);
        }

        // log create new record
        public static UserLog LogCreateDetails(string message, ApplicationUser actionLogger)
        {
            return new UserLog(message, null, Log.CREATE, actionLogger, null);
        }

        // not found
        public static UserLog LogNotFound(string errors, ApplicationUser actionLogger)
        {
            return new UserLog("Not Found", errors, Log.NOT_FOUND, actionLogger, null);
        }

        // log info
        public static UserLog LogInfoDetails(string message, ApplicationUser actionLogger, ApplicationUser user)
        {
            return new UserLog(message, null, Log.INFO, actionLogger, user);
        }

        // log info
        public static UserLog LogDeleteDetails(string message, ApplicationUser actionLogger, ApplicationUser user)
        {
            return new UserLog(message, null, Log.DELETE, actionLogger, user);
        }

        // log BadRequest
        public static UserLog LogBadRequests(string message, ApplicationUser actionLogger, ApplicationUser user)
        {
            return new UserLog("BadRequest!", message, Log.BAD_REQUEST, actionLogger, user);
        }

        // log error details
        public static UserLog LogErrorDetails(string error, ApplicationUser actionLogger, ApplicationUser user)
        {
            return new UserLog("Errors!", error, Log.ERROR, actionLogger, user);
        }

        #endregion
    }
}
