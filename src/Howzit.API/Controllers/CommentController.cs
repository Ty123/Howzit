using Howzit.API.Models;
using Howzit.Domains.Contracts;
using Howzit.Domains.Enums;
using Howzit.Domains.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Howzit.API.Controllers
{
    [RoutePrefix("api/Comment")]
    [Authorize]
    public class CommentController : ApiController
    {
        #region Private Members

        private readonly IUnitOfWork unitOfWork;

        #endregion

        public CommentController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region CRUD Methods

        [Route("Create")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(CommentViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                var user = unitOfWork.UserRepository.FindById(model.UserId);
                var task = unitOfWork.TaskRepository.FindById(model.TaskId);

                if (user == null || task == null)
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Task Found Or Invalid User", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("No Task Found!");
                }

                try
                {
                    var comment = new Comment(model.Message, task, user);
                    unitOfWork.CommentRepository.Add(comment);
                    unitOfWork.LogRepository.Add(new CommentLog("Comment On Task", null, Log.CREATE, actionLogger, comment));
                    unitOfWork.Commit();
                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest(ex.Message);
                }
            }

            var states = ModelState.SelectMany(x => x.Value.Errors).ToList();
            var errors = string.Empty;
            foreach (var error in states)
            {
                errors = string.Concat(errors, error.ErrorMessage + " | ");
            }

            unitOfWork.LogRepository.Add(new CommentLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);

        }

        [Route("Details/{id:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Details(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Comment Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.CommentRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new CommentLog("Comment Details", null, Log.INFO, actionLogger, row));
            unitOfWork.Commit();

            return Ok(row);
        }

        [Route("Edit")]
        [HttpPost]
        public async Task<IHttpActionResult> Edit(CommentViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                var row = unitOfWork.CommentRepository.FindById(model.Id);

                if (row == null)
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Comment Found", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("Not Found!");
                }

                row.Message = model.Message;
                row.UpdateBy = actionLogger.Id;
                row.Updated = DateTime.Now;

                try
                {
                    unitOfWork.CommentRepository.Edit(row);
                    unitOfWork.LogRepository.Add(new CommentLog("Update Comment!", null, Log.UPDATE, actionLogger, row));

                    unitOfWork.Commit();

                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                    unitOfWork.Commit();

                    return BadRequest(ex.Message);
                }
            }

            var states = ModelState.SelectMany(x => x.Value.Errors).ToList();
            var errors = string.Empty;

            foreach (var error in states)
            {
                errors = string.Concat(errors, error.ErrorMessage + " | ");
            }

            unitOfWork.LogRepository.Add(new TaskLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);

        }

        [Route("Delete/{id:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Delete(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Comment Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.CommentRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Comment Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return NotFound();
            }

            try
            {
                unitOfWork.CommentRepository.Delete(row);
                unitOfWork.LogRepository.Add(new CommentLog("Delete Comment!", null, Log.DELETE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Other Methods

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> All(int? taskId)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (taskId == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Bad Request!", "Invalid Task Id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid Task Id");

            }
            var comments = unitOfWork.CommentRepository.GetAll();
           
            try
            {
                if (comments == null || comments.Count() == 0)
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "No Comments", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();
                    return NotFound();
                }
                else
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Get Comments!", null, Log.INFO, actionLogger, null));
                    unitOfWork.Commit();
                    return Ok(comments);
                }

            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }
        }

        [Route("Approve/{id:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Approve(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;
            Comment row = null;

            try
            {
                if (id == null)
                {
                    unitOfWork.LogRepository.Add(new CommentLog("Bad Request!", "Invalid Task Id", Log.BAD_REQUEST, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("Invalid Task Id");
                }

                row = unitOfWork.CommentRepository.FindById(id.Value);

                row.IsApproved = true;
                row.UpdateBy = actionLogger.Id;
                row.Updated = DateTime.Now;

                unitOfWork.LogRepository.Add(new CommentLog("Approve comment!", null, Log.UPDATE, actionLogger, row));
                unitOfWork.CommentRepository.Edit(row);
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();
                
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
