using Howzit.API.Models;
using Howzit.Domains.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Howzit.API.Controllers
{
    [RoutePrefix("api/Supplier")]
    [Authorize]
    public class SupplierController : ApiController
    {
        #region Private Members 

        private readonly IUnitOfWork unitOfWork;

        #endregion

        public SupplierController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region CRUD Functions 

        [Route("Create")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(SupplierViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                //unitOfWork
            }

            var states = ModelState.SelectMany(n => n.Value.Errors).ToArray();

            var errors = string.Empty;

            foreach (var error in states)
            {
                errors = string.Concat(errors, error.ErrorMessage + "|");
            }

            //unitOfWork.LogRepository.Add(new ProjectLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));

            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        #endregion
    }
}
