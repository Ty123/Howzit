using Howzit.DAL.Repositories;
using Howzit.Domains.Models;
using Howzit.Domains.Contracts;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Data.Entity;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Howzit.DAL.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>, IUnitOfWork
    {

        #region Members

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Howzit.Domains.Models.Task> _taskRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<Logging> _logRepository;
        private readonly IRepository<Profile> _profileRepository;
        private readonly IRepository<Tool> _toolRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<UserTask> _userTaskRepository;
        private readonly IRepository<Owner> _ownerRepository;
        private readonly IRepository<Manager> _managerRepository;
        private readonly IRepository<Material> _materialRepository;

        #endregion

        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Database.Log = data => System.Diagnostics.Debug.WriteLine(data);

            #region Contracts

            _projectRepository = new Repository<Project>(this);
            _taskRepository = new Repository<Howzit.Domains.Models.Task>(this);
            _customerRepository = new Repository<Customer>(this);
            _contactRepository = new Repository<Contact>(this);
            _commentRepository = new Repository<Comment>(this);
            _logRepository = new Repository<Logging>(this);
            _profileRepository = new Repository<Profile>(this);
            _toolRepository = new Repository<Tool>(this);
            _userRepository = new Repository<ApplicationUser>(this);
            _userTaskRepository = new Repository<UserTask>(this);
            _ownerRepository = new Repository<Owner>(this);
            _managerRepository = new Repository<Manager>(this);
            _materialRepository = new Repository<Material>(this);

            #endregion
        }

        public void ForceDatabaseInitialize()
        {
            this.Database.Initialize(force: true);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<ApplicationUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<ApplicationUserLogin>().ToTable("UserLogins");

            #region Map Logging Tables

            modelBuilder.Entity<UserLog>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("UserLogs");
            });

            modelBuilder.Entity<TaskLog>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("TaskLogs");
            });

            modelBuilder.Entity<CommentLog>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("CommentLogs");
            });


            modelBuilder.Entity<CustomerLog>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("CustomerLogs");
            });

            modelBuilder.Entity<ProjectLog>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("ProjectLogs");
            });

            modelBuilder.Entity<CustomerContact>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("CustomerContacts");
            });

            modelBuilder.Entity<UserContact>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("UserContacts");
            });

            modelBuilder.Entity<SupplierContact>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("SupplierContacts");
            });

            #endregion
        }

        #region DbSet

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Howzit.Domains.Models.Task> Tasks { get; set; }
        public virtual DbSet<Tool> Tools { get; set; }
        public virtual DbSet<Logging> Loggings { get; set; }
        public virtual DbSet<Manager> Managers { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<TaskTool> TaskTools { get; set; }
        public virtual DbSet<UserTask> UserTasks { get; set; }
        public virtual DbSet<Owner> Owners { get; set; }
        public virtual DbSet<ApplicationUserLogin> UserLogins { get; set; }

        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<TaskMaterial> TaskMaterials { get; set; }

        #endregion

        #region IUnitOfWork Members

        public IRepository<Project> ProjectRepository
        {
            get { return _projectRepository; }
        }

        public IRepository<Howzit.Domains.Models.Task> TaskRepository
        {
            get { return _taskRepository; }
        }

        public IRepository<Customer> CustomerRepository
        {
            get { return _customerRepository; }
        }

        public IRepository<Contact> ContactRepository
        {
            get { return _contactRepository; }
        }

        public IRepository<Comment> CommentRepository
        {
            get { return _commentRepository; }
        }

        public ApplicationUserManager ApplicationUserManager
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ApplicationUserManager)) as ApplicationUserManager; }
        }

        public IRepository<Logging> LogRepository
        {
            get { return _logRepository; }
        }

        public IRepository<Profile> ProfileRepository
        {
            get { return _profileRepository; }
        }

        public IRepository<Tool> ToolRepository
        {
            get { return _toolRepository; }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAuthenticationManager)) as IAuthenticationManager; }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISecureDataFormat<AuthenticationTicket>)) as ISecureDataFormat<AuthenticationTicket>; }
        }

        public IRepository<ApplicationUser> UserRepository
        {
            get { return _userRepository; }
        }

        public IRepository<UserTask> UserTaskRepository
        {
            get { return _userTaskRepository; }
        }

        public IRepository<Owner> OwnerRepository
        {
            get { return _ownerRepository; }
        }

        public Task<ApplicationUser> CurrentUser
        {
            get
            {
                var userId = (GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IHttpContextBaseRepository)) as IHttpContextBaseRepository).HttpContext.User.Identity.GetUserId<int>();

                return this.ApplicationUserManager.FindByIdAsync(userId);
            }
        }

        public IRepository<Manager> ManagerRepository
        {
            get { return _managerRepository; }
        }

        public IRepository<Material> MaterialRepository
        {
            get
            {
                return _materialRepository;
            }
        }

        public void Commit()
        {
            this.SaveChanges();
        }

        #endregion
    }
}
