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
    public static class LogAction
    {
        //#region Methods

        //// log invalid model states
        //public static Logging LogModelStateError(this Logging loggin, ApiController controller, ApplicationUser user, out string errors)
        //{
        //    var modelErrors = controller.ModelState.Select(x => x.Value.Errors).Where(y => y.Count() > 0).FirstOrDefault();
        //    string mErrors = "";

        //    foreach (var error in modelErrors)
        //    {
        //        mErrors = string.Concat(mErrors, error.ErrorMessage + " | ");
        //    }

        //    errors = mErrors;

        //    return new Logging("ModelState Errors!", mErrors, Log.ERROR, user);
        //}

        //// log exception details
        //public static Logging LogExceptionDetails(this Logging loggin, Exception ex, ApplicationUser user)
        //{
        //    return new Logging("Exception thrown!", ex.Message, Log.EXCEPTION, user);
        //}

        //// log update a record
        //public static Logging LogUpdateDetails(this Logging loggin, string message, ApplicationUser user)
        //{
        //    return new Logging(message, null, Log.UPDATE, user);
        //}

        //// log create new record
        //public static Logging LogCreateDetails(this Logging loggin, string message, ApplicationUser user)
        //{
        //    return new Logging(message, null, Log.CREATE, user);
        //}

        //// not found
        //public static Logging LogNotFound(this Logging loggin, string errors, ApplicationUser user)
        //{
        //    return new Logging("Not Found", errors, Log.NOT_FOUND, user);
        //}

        //// log info
        //public static Logging LogInfoDetails(this Logging loggin, string message, ApplicationUser user)
        //{
        //    return new Logging(message, null, Log.INFO, user);
        //}

        //// log info
        //public static Logging LogDeleteDetails(this Logging loggin, string message, ApplicationUser user)
        //{
        //    return new Logging(message, null, Log.DELETE, user);
        //}

        //// log BadRequest
        //public static Logging LogBadRequests(this Logging loggin, string message, ApplicationUser user)
        //{
        //    return new Logging("BadRequest!", message, Log.BAD_REQUEST, user);
        //}

        //// log error details
        //public static Logging LogErrorDetails(this Logging loggin, string error, ApplicationUser user)
        //{
        //    return new Logging("Errors!", error, Log.ERROR, user);
        //}


        //#endregion
    }
}
