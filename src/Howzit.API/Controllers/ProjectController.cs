using Howzit.API.Models;
using Howzit.Domains.Contracts;
using Howzit.Domains.Enums;
using Howzit.Domains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Howzit.API.Controllers
{
    [RoutePrefix("api/Project")]
    [Authorize]
    public class ProjectController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public ProjectController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region CRUD Methods

        // api/Project/Create
        [Route("Create")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(ProjectViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {

                var customer = unitOfWork.CustomerRepository.FindById(model.CustomerId);
                var manager = unitOfWork.UserRepository.FindById(model.ManagerId);

                if (customer == null || manager == null)
                {

                    unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", string.Format("No manger id=[{0}] or customer id=[{1}]", model.ManagerId, model.CustomerId), Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("Not Found!");
                }

                try
                {
                    unitOfWork.ProjectRepository.Add(new Project(model.Name, model.Description, new Manager(manager), customer, State.InProgress, actionLogger));

                    unitOfWork.LogRepository.Add(new ProjectLog("Create New Project", string.Format("No manger id=[{0}] or customer id=[{1}]", model.ManagerId, model.CustomerId), Log.CREATE, actionLogger, null));
                    unitOfWork.Commit();

                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new ProjectLog("Exception thrown!", string.Format("No manger id=[{0}] or customer id=[{1}]", model.ManagerId, model.CustomerId), Log.EXCEPTION, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest(ex.Message);
                }
            }

            var messages = ModelState.SelectMany(n => n.Value.Errors).ToArray();
            var errors = string.Empty;
            foreach (var error in messages)
            {
                errors = string.Concat(errors, error.ErrorMessage + "|");
            }

            unitOfWork.LogRepository.Add(new ProjectLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        // api/Project/Details/5
        [Route("Details/{id:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Details(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid Project Id");
            }

            var row = unitOfWork.ProjectRepository.Get(c => c.Id == id, includeProperties: "Manager,Customer,Tasks").FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No customer id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new ProjectLog("Get customer details!", null, Log.INFO, actionLogger, row));
            unitOfWork.Commit();

            return Ok(new
            {
                Id = row.Id,
                Name = row.Name,
                Description = row.Description,
                ProjectState = row.State,
                Created = row.Created,
                CreatedBy = row.CreateBy,
                Update = row.Updated,
                UpdateBy = row.UpdateBy,
                ManagerId = row.ManagerId,
                CustomerId = row.CustomerId
            });

        }

        // api/Project/Update
        [Route("Update")]
        [HttpPost]
        public async Task<IHttpActionResult> Update(ProjectViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {

                try
                {
                    var project = unitOfWork.ProjectRepository.FindById(model.Id);

                    if (project == null)
                    {
                        unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No customer id=[" + model.Id + "]", Log.NOT_FOUND, actionLogger, null));
                        unitOfWork.Commit();

                        return NotFound();
                    }

                    project.Name = model.Name;
                    project.Description = model.Description;
                    project.UpdateBy = actionLogger.Id;
                    project.Updated = DateTime.Now;

                    unitOfWork.ProjectRepository.Edit(project);
                    unitOfWork.LogRepository.Add(new ProjectLog("Update project!", "Update customer id=[" + model.Id + "]", Log.UPDATE, actionLogger, project));
                    unitOfWork.Commit();

                    return Ok();

                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new ProjectLog("Exception update cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest(ex.Message);
                }
            }

            var messages = ModelState.SelectMany(n => n.Value.Errors).ToArray();
            var errors = string.Empty;
            foreach (var error in messages)
            {
                errors = string.Concat(errors, error.ErrorMessage + "|");
            }

            unitOfWork.LogRepository.Add(new ProjectLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        // api/Project/Delete/5
        [Route("Delete/{id:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Bad request!", "Invalid project id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid customer id");
            }

            var project = unitOfWork.ProjectRepository.FindById(id.Value);

            if (project == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No project id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            try
            {
                unitOfWork.ProjectRepository.Delete(project);
                unitOfWork.LogRepository.Add(new ProjectLog("Remove customer!", null, Log.DELETE, actionLogger, project));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Exception delete cusomter!", ex.Message, Log.EXCEPTION, actionLogger, project));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Other Methods

        [Route("GetBy")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> All(int? managerId, int? customerId)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            List<Project> rows = null;
            if (managerId == null && customerId == null)
            {
                rows = unitOfWork.ProjectRepository.Get(includeProperties: "Manager,Customer").ToList();
            }
            else if (managerId != null && customerId == null)
            {
                rows = unitOfWork.ProjectRepository.Get(p => p.ManagerId == managerId, includeProperties: "Manager,Customer").ToList();
            }
            else if (managerId == null && customerId != null)
            {
                rows = unitOfWork.ProjectRepository.Get(p => p.CustomerId == customerId, includeProperties: "Manager,Customer").ToList();
            }
            else
            {
                rows = unitOfWork.ProjectRepository.Get(p => p.CustomerId == customerId && p.ManagerId == managerId, includeProperties: "Manager,Customer").ToList();
            }

            if (rows == null || rows.Count() == 0)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No Project Found.", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Not Found!");
            }

            var projects = rows.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ProjectState = p.State,
                Created = p.Created,
                CreatedBy = p.CreateBy,
                Update = p.Updated,
                UpdateBy = p.UpdateBy,
                ManagerId = p.ManagerId,
                CustomerId = p.CustomerId
            }).ToList();


            unitOfWork.LogRepository.Add(new ProjectLog("Get All Projects", null, Log.INFO, actionLogger, null));
            unitOfWork.Commit();

            return Ok(projects);

        }

        [Route("{id:int}/Close")]
        [HttpGet]
        public async Task<IHttpActionResult> CloseProject(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Bad Request!", "Invalid Project Id.", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid Project Id!");
            }

            var row = unitOfWork.ProjectRepository.Get(includeProperties: "Tasks").Where(p => p.Id == id.Value).FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No row id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            if (row.Tasks.Count() > 0)
            {
                foreach (var task in row.Tasks)
                {
                    if (task.State == State.InProgress)
                    {
                        unitOfWork.LogRepository.Add(new ProjectLog("Failure Close Task", "Unable to close project id=[" + row.Id + "]", Log.ERROR, actionLogger, row));
                        unitOfWork.Commit();

                        return BadRequest("Unable to close project id=[" + row.Id + "]. Task id=[" + task.Id + "] in progress.");
                    }
                }
            }

            var message = string.Format("User id=[{0}] close project id=[{1}]", actionLogger.Id, id);
            row.State = State.Completed;
            unitOfWork.ProjectRepository.Edit(row);
            unitOfWork.LogRepository.Add(new ProjectLog("CLose Project!", message, Log.UPDATE, actionLogger, row));
            unitOfWork.Commit();

            return Ok(row);
        }

        [Route("{id:int}/Restart")]
        [HttpGet]
        public async Task<IHttpActionResult> RestartProject(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Bad Request!", "Invalid Project Id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.ProjectRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No row id=[" + id + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            try
            {
                row.State = State.InProgress;
                unitOfWork.ProjectRepository.Edit(row);
                unitOfWork.LogRepository.Add(new ProjectLog("Restart Project", null, Log.UPDATE, actionLogger, row));
                unitOfWork.Commit();

                return Ok(row);
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add((new ProjectLog("Restart Project", ex.Message, Log.EXCEPTION, actionLogger, row)));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/ChangeManager")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangeManager(int? id, ChangeManagerModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new ProjectLog("Bad Request!", "Invalid Project Id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            if (ModelState.IsValid)
            {

                var row = unitOfWork.ProjectRepository.FindById(id.Value);

                if (row == null)
                {
                    unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", "No project id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return NotFound();
                }

                var olderManager = unitOfWork.ManagerRepository.FindById(model.OldManagerId);
                var manager = unitOfWork.UserRepository.FindById(model.NewManagerId);

                if (olderManager == null || manager == null)
                {
                    unitOfWork.LogRepository.Add(new ProjectLog("Not Found!", string.Format("No manger id=[{0}] or new manager id=[{1}]", model.OldManagerId, model.NewManagerId), Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("Not Found!");
                }

                try
                {
                    var newManager = new Manager(manager);
                    newManager.Projects.Add(row);
                    unitOfWork.ManagerRepository.Add(newManager);
                    unitOfWork.LogRepository.Add(new ProjectLog("Create New Project", string.Format("Change manger id=[{0}] to manager id=[{1}]", model.OldManagerId, model.NewManagerId), Log.CREATE, actionLogger, null));
                    unitOfWork.Commit();

                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new ProjectLog("Exception thrown!", string.Format("Unable to manger id=[{0}] to new manager id=[{1}]", model.OldManagerId, model.NewManagerId), Log.EXCEPTION, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest(ex.Message);
                }
            }

            var messages = ModelState.SelectMany(n => n.Value.Errors).ToArray();
            var errors = string.Empty;
            foreach (var error in messages)
            {
                errors = string.Concat(errors, error.ErrorMessage + "|");
            }

            unitOfWork.LogRepository.Add(new ProjectLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);

        }

        #endregion
    }
}
