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
    [RoutePrefix("api/Customer")]
    [Authorize]
    public class CustomerController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region CRUD Methods

        // api/Customer/Create
        [Route("Create")]
        [HttpPost]
        public async Task<IHttpActionResult> Create(CustomerViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {
                try
                {
                    unitOfWork.CustomerRepository.Add(new Customer(model.Name, model.CompanyName, model.Website, actionLogger));
                    unitOfWork.LogRepository.Add(new CustomerLog("Create new customer!", "New customer name=[" + model.Name + "]", Log.CREATE, actionLogger, null));

                    unitOfWork.Commit();

                    return new StatusCodeResult(HttpStatusCode.Created, Request);

                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Exception thrown when create cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
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

            unitOfWork.LogRepository.Add(new CustomerLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        // api/Customer/Details/5
        [Route("Details/{id:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Details(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid customer id");
            }

            var row = unitOfWork.CustomerRepository.Get(includeProperties:"CustomerContacts").FirstOrDefault();
            
            var model = new
            {
                Id = row.Id,
                Name = row.Name,
                CompanyName = row.CompanyName,
                Website = row.Website,
                Contacts = row.CustomerContacts.Select(c => new
                {
                    Id = c.Id,
                    Mr = c.Initial,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    MiddleName = c.MiddleName,
                    Phone = c.Phone,
                    Email = c.Email
                }).ToList()
            };

            if (row == null)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Not Found!", "No customer id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            unitOfWork.LogRepository.Add(new CustomerLog("Get customer details!", null, Log.INFO, actionLogger, row));
            unitOfWork.Commit();

            return Ok(model);
        }

        // api/Customer/Update
        [Route("Update")]
        [HttpPost]
        public async Task<IHttpActionResult> Update(CustomerViewModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (ModelState.IsValid)
            {

                try
                {

                    var customer = unitOfWork.CustomerRepository.FindById(model.Id);

                    if (customer == null)
                    {
                        unitOfWork.LogRepository.Add(new CustomerLog("Not Found!", "No customer id=[" + model.Id + "]", Log.NOT_FOUND, actionLogger, null));
                        unitOfWork.Commit();

                        return NotFound();
                    }

                    customer.Name = model.Name;
                    customer.Website = model.Website;
                    customer.CompanyName = model.CompanyName;
                    customer.UpateBy = actionLogger.Id;
                    customer.Updated = DateTime.Now;

                    unitOfWork.CustomerRepository.Edit(customer);
                    unitOfWork.LogRepository.Add(new CustomerLog("Update customer!", "Update customer id=[" + model.Id + "]", Log.UPDATE, actionLogger, customer));
                    unitOfWork.Commit();

                    return Ok();

                }
                catch (Exception ex)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Exception update cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
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

            unitOfWork.LogRepository.Add(new CustomerLog("Invalid model state!", errors, Log.ERROR, actionLogger, null));
            unitOfWork.Commit();

            return BadRequest(ModelState);
        }

        // api/Customer/Delete/5
        [Route("Delete/{id:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid customer id");
            }

            var customer = unitOfWork.CustomerRepository.FindById(id.Value);

            if (customer == null)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Not Found!", "No customer id=[" + id.Value + "]", Log.NOT_FOUND, actionLogger, null));
                unitOfWork.Commit();

                return NotFound();
            }

            try
            {
                unitOfWork.CustomerRepository.Delete(customer);
                unitOfWork.LogRepository.Add(new CustomerLog("Remove customer!", null, Log.DELETE, actionLogger, customer));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Exception delete cusomter!", ex.Message, Log.EXCEPTION, actionLogger, customer));
                unitOfWork.Commit();
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Other Methods

        // api/Customer
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> All()
        {
            var actionLogger = await unitOfWork.CurrentUser;
            var customers = unitOfWork.CustomerRepository.GetAll().Select(c => new
            {
                Id = c.Id,
                Name = c.Name,
                CompanyName = c.CompanyName,
                Website = c.Website
            }).ToList();

            try
            {
                if (customers == null && customers.Count() > 0)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Not Found!", "No Customers Found", Log.NOT_FOUND, actionLogger, null));
                    unitOfWork.Commit();

                    return NotFound();
                }
                else
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("All Customers", null, Log.INFO, actionLogger, null));
                    unitOfWork.Commit();

                    return Ok(customers);
                }

            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Exception thrown", ex.Message, Log.EXCEPTION, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        // api/Customer/5/AddContacts
        [Route("{id:int}/AddContacts")]
        [HttpPost]
        public async Task<IHttpActionResult> AddContacts(int? id, [FromBody] List<CustomerContactModel> models)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            try
            {

                if (id == null)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("Invalid customer id");

                }

                var customer = unitOfWork.CustomerRepository.FindById(id.Value);

                if (models == null || models.Count() == 0)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer contacts", Log.BAD_REQUEST, actionLogger, customer));
                    unitOfWork.Commit();

                    return BadRequest("Invalid customer id");

                }

                foreach (var model in models)
                {
                    unitOfWork.ContactRepository.Add(new CustomerContact(model.FirstName, model.LastName, model.MiddleName, model.Initial, model.Position, model.IsPrimary, model.Phone, model.Email, customer, actionLogger));
                }

                unitOfWork.LogRepository.Add(new CustomerLog("Add Customer Contacts", null, Log.UPDATE, actionLogger, customer));
                unitOfWork.Commit();

                return new StatusCodeResult(HttpStatusCode.Created, Request);

            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Exception thrown when create cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        // api/Customer/5/RemoveContacts
        [Route("{id:int}/RemoveContacts")]
        [HttpPost]
        public async Task<IHttpActionResult> RemoveContacts(int? id, [FromBody] List<CustomerContactModel> models)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            try
            {
                if (id == null)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("Invalid customer id");
                }

                var customer = unitOfWork.CustomerRepository.FindById(id.Value);

                if (models == null || models.Count() == 0)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer contacts", Log.BAD_REQUEST, actionLogger, customer));
                    unitOfWork.Commit();

                    return BadRequest("Invalid customer id");
                }

                foreach (var model in models)
                {
                    var contact = unitOfWork.ContactRepository.FindById(model.Id);
                    unitOfWork.ContactRepository.Delete(contact);
                }

                unitOfWork.LogRepository.Add(new CustomerLog("Remove Customer Contacts", null, Log.UPDATE, actionLogger, customer));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Exception thrown when create cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/UpdateContacts")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateContact(int? id, [FromBody] CustomerContactModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            try
            {
                if (id == null)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                    unitOfWork.Commit();

                    return BadRequest("Invalid customer id");
                }

                var customer = unitOfWork.CustomerRepository.FindById(id.Value);

                if (!ModelState.IsValid)
                {
                    // logging model state errors
                    var messages = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                    var errors = string.Join(" | ", messages);

                    unitOfWork.LogRepository.Add(new CustomerLog("Invalid model state!", errors, Log.ERROR, actionLogger, customer));
                    unitOfWork.Commit();

                }

                var contact = unitOfWork.ContactRepository.FindById(model.Id);
                if (contact == null)
                {
                    unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid contact id", Log.NOT_FOUND, actionLogger, customer));
                    unitOfWork.Commit();

                    return BadRequest("Invalid contact id");
                }

                contact.FirstName = model.FirstName;
                contact.LastName = model.LastName;
                contact.MiddleName = model.MiddleName;
                contact.Initial = model.Initial;
                contact.IsPrimary = model.IsPrimary;
                contact.Phone = model.Phone;

                unitOfWork.LogRepository.Add(new CustomerLog("Remove Customer Contacts", null, Log.UPDATE, actionLogger, customer));
                unitOfWork.Commit();

                return Ok();
            }
            catch (Exception ex)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Exception thrown when create cusomter!", ex.Message, Log.EXCEPTION, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest(ex.Message);
            }
        }

        [Route("{id:int}/AllContacts")]
        [HttpGet]
        public async Task<IHttpActionResult> AllContacts(int? id)
        {
            var actionLogger = await unitOfWork.CurrentUser;

            if (id == null)
            {
                unitOfWork.LogRepository.Add(new CustomerLog("Bad request!", "Invalid customer id", Log.BAD_REQUEST, actionLogger, null));
                unitOfWork.Commit();

                return BadRequest("Invalid customer id");
            }

            var customer = unitOfWork.CustomerRepository.Get(includeProperties: "CustomerContacts").Where(c => c.Id == id.Value).FirstOrDefault();

            var contacts = new List<CustomerContact>();

            if (customer.CustomerContacts != null && customer.CustomerContacts.Count() > 0)
            {

                foreach (var contact in customer.CustomerContacts)
                {
                    if (contact != null)
                        contacts.Add(contact);
                }
            }
            else
            {
                unitOfWork.LogRepository.Add(new CustomerLog("No Contacts Found!", null, Log.INFO, actionLogger, customer));
                unitOfWork.Commit();

                return BadRequest("No Contacts Found!");
            }

            unitOfWork.LogRepository.Add(new CustomerLog("Get Customer Contact", null, Log.INFO, actionLogger, customer));
            unitOfWork.Commit();

            return Ok(contacts);

        }

        #endregion
    }
}
