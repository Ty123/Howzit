using Howzit.Domains.Models;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Project> ProjectRepository { get; }
        IRepository<Howzit.Domains.Models.Task> TaskRepository { get; }
        IRepository<Customer> CustomerRepository { get; }
        IRepository<Contact> ContactRepository { get; }
        IRepository<Comment> CommentRepository { get; }
        ApplicationUserManager ApplicationUserManager { get; }
        Task<ApplicationUser> CurrentUser { get; }
        IRepository<Logging> LogRepository { get; }
        IRepository<Profile> ProfileRepository { get; }
        IRepository<Tool> ToolRepository { get; }
        IAuthenticationManager AuthenticationManager { get; }
        ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; }
        IRepository<ApplicationUser> UserRepository { get; }
        IRepository<UserTask> UserTaskRepository { get; }
        IRepository<Owner> OwnerRepository { get; }
        IRepository<Manager> ManagerRepository { get; }
        IRepository<Material> MaterialRepository { get; }

        void Commit();
    }
}
