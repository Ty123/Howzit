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
    [Authorize]
    [RoutePrefix("api/User")]
    public class UserProfileController : ApiController
    {

        #region Private Members

        private readonly IUnitOfWork unitOfWork;

        #endregion

        public UserProfileController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region Profile Functions

        [Route("{id:int}/CreateProfile")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(int? id, ProfileBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "User Not Found!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid User Id!");
            }


            if (ModelState.IsValid)
            {
                var user = unitOfWork.UserRepository.FindById(id.Value);

                if (user == null)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User Not Found", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("User Not Found!");
                }

                try
                {
                    unitOfWork.ProfileRepository.Add(new Profile(model.FirstName, model.LastName, model.MiddleName, model.Initial, model.Gender, model.Phone, model.Address, user));
                    unitOfWork.LogRepository.Add(new UserLog("Create Profile!", null, Log.UPDATE, actionLogger, user));
                    unitOfWork.Commit();
                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, user));
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

            unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", errors, Log.ERROR, actionLogger, actionLogger));
            unitOfWork.Commit();

            return BadRequest(ModelState);

        }

        [Route("{id:int}/ProfileDetails")]
        [HttpGet]
        public async Task<IHttpActionResult> Details(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User Not Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.ProfileRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User Not Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new UserLog("User Profile", null, Log.INFO, actionLogger, row.User));
            unitOfWork.Commit();

            return Ok(new
            {
                Id = row.UserId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                MiddleName = row.MiddleName,
                Gender = row.Gender,
                Address = row.Address,
                Phone = row.Phone
            });
        }

        [Route("{id:int}/EditProfile")]
        [HttpPost]
        public async Task<IHttpActionResult> Edit(ProfileBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                var row = unitOfWork.ProfileRepository.FindById(model.UserId);

                if (row == null)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No Comment Found", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("Not Found!");
                }

                row.Initial = model.Initial;
                row.FirstName = model.FirstName;
                row.LastName = model.LastName;
                row.MiddleName = model.MiddleName;
                row.Address = model.Address;
                row.Gender = model.Gender;
                row.Phone = model.Phone;
                row.UpdateBy = actionLogger.Id;
                row.Updated = DateTime.Now;

                try
                {
                    unitOfWork.ProfileRepository.Edit(row);
                    unitOfWork.LogRepository.Add(new UserLog("Update Comment!", null, Log.UPDATE, actionLogger, row.User));

                    unitOfWork.Commit();

                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row.User));
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

        [Route("{id:int}/DeleteProfile")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User Not Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("Invalid id");
            }

            var row = unitOfWork.ProfileRepository.FindById(id.Value);

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new CommentLog("Not Found!", "Profile Not Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();
                return NotFound();
            }

            try
            {
                unitOfWork.ProfileRepository.Delete(row);
                unitOfWork.LogRepository.Add(new UserLog("Delete Profile!", null, Log.DELETE, actionLogger, row.User));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row.User));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Contact Functions

        [Route("{id:int}/AddContacts")]
        [HttpPost]
        public async Task<IHttpActionResult> AddContact(int? id, List<UserContactModel> models)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "User id is null!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("User id is null!");
            }

            if (models == null || models.Count() == 0)
            {
                unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "Contact models is null or empty!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Contact models is null or empty!");
            }

            var user = unitOfWork.UserRepository.FindById(id.Value);

            if (user == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No Task Found Or Invalid User", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("User Not Found!");
            }

            foreach (var model in models)
            {
                if (model == null)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "A Contact is null!", Log.BAD_REQUEST, actionLogger, null));
                    unitOfWork.Commit();
                    return BadRequest("A Contact is null!");
                }

                user.Contacts.Add(new UserContact(model.FirstName, model.LastName, model.MiddleName, model.Initial, model.Phone, model.Email, model.IsPrimary, model.Relationship, user, actionLogger));
            }

            try
            {
                unitOfWork.LogRepository.Add(new UserLog("Create User Contacts!", null, Log.UPDATE, actionLogger, user));
                unitOfWork.Commit();
                return new StatusCodeResult(HttpStatusCode.Created, Request);
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/EditContact")]
        [HttpPost]
        public async Task<IHttpActionResult> EditContact(int? id, int? contactId, [FromBody] UserContactModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "User id is null!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("User id is null!");
            }

            if (contactId == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Bad Request!", "Contact id is null!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Contact id is null!");
            }

            var user = unitOfWork.UserRepository.Get(u => u.Id == id.Value, includeProperties: "Contacts").FirstOrDefault();

            if (user == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No User Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("User Not Found!");
            }

            var contact = user.Contacts.Where(c => c.Id == contactId.Value).FirstOrDefault();

            if (contact == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No Contact Found", Log.NOT_FOUND, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest("Contact Not Found!");
            }

            if (ModelState.IsValid)
            {

                contact.FirstName = model.FirstName;
                contact.LastName = model.LastName;
                contact.MiddleName = model.MiddleName;
                contact.Initial = model.Initial;
                contact.Relationship = model.Relationship;
                contact.Email = model.Email;
                contact.Phone = model.Phone;

                try
                {
                    unitOfWork.ContactRepository.Edit(contact);
                    unitOfWork.LogRepository.Add(new UserLog("Edit User Contacts!", null, Log.UPDATE, actionLogger, user));
                    unitOfWork.Commit();
                    return new StatusCodeResult(HttpStatusCode.Created, Request);
                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, user));
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

            unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", errors, Log.ERROR, actionLogger, actionLogger));

            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        [Route("{id:int}/ContactDetails")]
        [HttpGet]
        public async Task<IHttpActionResult> ContactDetails(int? id, int? contactId)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null && contactId == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User or contact id is null!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("User or contact id is null!");
            }

            var row = unitOfWork.UserRepository.Get(u=>u.Id==id.Value,includeProperties:"Contacts").FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No User Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            var contact = row.Contacts.Where(c=>c.Id == contactId.Value).FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No Contact Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new UserLog("User Contact", null, Log.INFO, actionLogger, null));
            unitOfWork.Commit();

            return Ok(new
            {
                Id = contact.Id,
                Initial = contact.Initial,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Relationship = contact.Relationship,
                Phone = contact.Phone,
                UserId = row.Id
            });
        }

        [Route("{id:int}/DeleteContact")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteContact(int? id, int? contactId)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null && contactId == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "User or contact id is null!", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();
                return BadRequest("User or contact id is null!");
            }

            var row = unitOfWork.UserRepository.Get(u => u.Id == id.Value, includeProperties: "Contacts").FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No User Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            var contact = row.Contacts.Where(c => c.Id == contactId.Value).FirstOrDefault();

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Not Found!", "No Contact Found", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            try
            {
                unitOfWork.ContactRepository.Delete(contact);
                unitOfWork.LogRepository.Add(new UserLog("Delete Contact!", null, Log.DELETE, actionLogger, row));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new UserLog("Exception!", ex.Message, Log.EXCEPTION, actionLogger, row));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
