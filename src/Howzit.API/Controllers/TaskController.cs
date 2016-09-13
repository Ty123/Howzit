using Howzit.API.Models;
using Howzit.Domains.Contracts;
using Howzit.Domains.Enums;
using Howzit.Domains.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Howzit.API.Controllers
{
    [RoutePrefix("api/Task")]
    [Authorize]
    public class TaskController : ApiController
    {

        private readonly IUnitOfWork unitOfWork;

        public TaskController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region CRUD Methods

        [Route("Create")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(TaskViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                var user = unitOfWork.UserRepository.FindById(model.OwnerId);
                var project = unitOfWork.ProjectRepository.FindById(model.ProjectId);

                if (user == null || project == null)
                {

                    unitOfWork.LogRepository.Add(new TaskLog("Not Found!","No Owner Or Project Found",Log.NOT_FOUND,actionLogger,null));
                    unitOfWork.Commit();

                    return BadRequest("No Owner Or Project Found");
                }

                try
                {
                    // new owner or existing owner
                    var owner = (unitOfWork.OwnerRepository.FindById(model.OwnerId)) ?? new Owner(user); 

                    unitOfWork.TaskRepository.Add(new Howzit.Domains.Models.Task(model.Summary, model.Description, model.State, model.Progress, model.Priority, project, owner,model.Start,model.End, actionLogger));
                    unitOfWork.LogRepository.Add(new TaskLog("Create New Task", null, Log.CREATE, actionLogger, null));
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

            unitOfWork.LogRepository.Add(new TaskLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
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
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new TaskLog("Task Details", null, Log.INFO, actionLogger, row));
            unitOfWork.Commit();

            return Ok(row);
        }

        [Route("Edit")]
        [HttpPost]
        public async Task<IHttpActionResult> Edit(TaskViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                var row = unitOfWork.TaskRepository.FindById(model.Id);

                if (row == null)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("Not Found!");
                }

                row.Summary = model.Summary;
                row.Description = model.Description;
                row.DateStart = model.Start;
                row.DateEnd = model.End;
                row.State = model.State;
                row.UpdateBy = actionLogger.Id;
                row.Updated = DateTime.Now;

                try
                {
                    unitOfWork.TaskRepository.Edit(row);
                    unitOfWork.LogRepository.Add(new TaskLog("Exception!", null, Log.UPDATE, actionLogger, row));

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
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return NotFound();
            }

            unitOfWork.TaskRepository.Delete(row);
            unitOfWork.LogRepository.Add(new TaskLog("Delete Task!", null, Log.DELETE, actionLogger, row));
            unitOfWork.Commit();

            return Ok(row);
        }

        #endregion

        #region Other Methods

        [Route("{id:int}/AssignTask")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignTask(int? id, List<TaskAssignModel> models)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            if (models == null || models.Count() == 0)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Assignee Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            List<UserTask> assignees = new List<UserTask>();

            foreach (var model in models)
            {
                if(model != null) {
                    var user = unitOfWork.UserRepository.FindById(model.UserId);
                    if(user != null){
                        assignees.Add(new UserTask(user));
                    }
                }
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Assignee Found", Log.BAD_REQUEST, actionLogger, row));
            }

            try
            {
                row.UserTasks = assignees;
                unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();                
                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/CloseTask")]
        [HttpPost]
        public async Task<IHttpActionResult> CloseTask(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            if (row.State == State.Completed)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "Task Is Already Closed", Log.BAD_REQUEST, actionLogger, row));
                unitOfWork.Commit();

                return BadRequest();
            }

            row.State = State.Completed;
            row.UpdateBy = actionLogger.Id;
            row.Updated = DateTime.Now;

            try
            {
                unitOfWork.TaskRepository.Edit(row);
                unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/ReopenTask")]
        [HttpPost]
        public async Task<IHttpActionResult> ReOpenTask(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            if (row.State == State.InProgress)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "Task Is In Progress", Log.BAD_REQUEST, actionLogger, row));
                unitOfWork.Commit();

                return BadRequest();
            }

            row.State = State.InProgress;
            row.UpdateBy = actionLogger.Id;
            row.Updated = DateTime.Now;

            try
            {
                unitOfWork.TaskRepository.Edit(row);
                unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/ChangeOwner")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangeOwner(int? id, ChangeTaskOwnerModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            if (ModelState.IsValid)
            {
                var olderOwner = unitOfWork.OwnerRepository.FindById(model.OldOwnerId);
                var user = unitOfWork.UserRepository.FindById(model.NewOwnerId);
                var row = unitOfWork.TaskRepository.FindById(id.Value);

                if (row == null && row.State == State.Completed && olderOwner == null && user == null)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found, Closed Or User Not Exist", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return NotFound();
                }

                try
                {

                    row.Owner = new Owner(user);
                    row.UpdateBy = actionLogger.Id;
                    row.Updated = DateTime.Now;

                    unitOfWork.TaskRepository.Edit(row);
                    unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                    unitOfWork.Commit();

                    return Ok();
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                    unitOfWork.Commit();
                    return BadRequest(ex.Message);
                }
            }

            var messages = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
            var errors = string.Join(" | ", messages);

            unitOfWork.LogRepository.Add(new TaskLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);

        }


        [Route("{id:int}/ChangeAssignee")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangeAssignee(int? id, ChangeTaskAssigneeModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            if (ModelState.IsValid)
            {
                var oldAssignee = unitOfWork.UserTaskRepository.FindById(model.OldUserId);
                var user = unitOfWork.UserRepository.FindById(model.NewUserId);
                var row = unitOfWork.TaskRepository.FindById(id.Value);

                if (row == null && row.State == State.Completed && oldAssignee == null && user == null)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found, Closed Or User Not Exist", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return NotFound();
                }

                try
                {
                    row.UserTasks.Add(new UserTask(user));
                    row.UpdateBy = actionLogger.Id;
                    row.Updated = DateTime.Now;

                    unitOfWork.UserTaskRepository.Delete(oldAssignee);
                    unitOfWork.TaskRepository.Edit(row);
                    unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                    unitOfWork.Commit();

                    return Ok();
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
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

        [Route("{id:int}/RemoveAssignee")]
        [HttpGet]
        public async Task<IHttpActionResult> RemoveAssignee(int? id, int? assignee)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null && assignee == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Bad Request!", "No Task Found", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var oldAssignee = unitOfWork.UserTaskRepository.FindById(assignee.Value);
            var row = unitOfWork.TaskRepository.FindById(id.Value);

            if (row == null && row.State == State.Completed && oldAssignee == null)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Not Found!", "No Task Found, Closed Or User Not Exist", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            try
            {
                row.UserTasks.Remove(oldAssignee);
                row.UpdateBy = actionLogger.Id;
                row.Updated = DateTime.Now;

                unitOfWork.TaskRepository.Edit(row);
                unitOfWork.LogRepository.Add(new TaskLog("Assign Task", null, Log.UPDATE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new TaskLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }

        }

        #endregion
    }
}
