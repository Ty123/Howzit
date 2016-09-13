[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Howzit.API.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Howzit.API.App_Start.NinjectWebCommon), "Stop")]

namespace Howzit.API.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Howzit.Domains.Contracts;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.DataProtection;
    using Microsoft.Owin.Security.OAuth;
    using Microsoft.Owin.Security.DataHandler;
    using Microsoft.Owin.Security.DataHandler.Encoder;
    using Microsoft.Owin.Security.DataHandler.Serializer;
    using Howzit.API.Providers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Howzit.DAL.Repositories;
    using Howzit.DAL.Context;
    using Howzit.Domains.Models;
    using System.Data.Entity;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            #region DbContext

            kernel.Bind<IUnitOfWork>().To<ApplicationDbContext>();
            kernel.Bind<DbContext>().To<ApplicationDbContext>();
            //kernel.Bind<ApplicationDbContext>().ToSelf();

            #endregion

            #region Token
            kernel.Bind<IAuthenticationManager>().ToMethod(c => HttpContext.Current.GetOwinContext().Authentication);
            kernel.Bind<IDataProtectionProvider>().To<DpapiDataProtectionProvider>();
            kernel.Bind<IOAuthAuthorizationServerProvider>().To<ApplicationOAuthProvider>();
            kernel.Bind<ISecureDataFormat<AuthenticationTicket>>().To<SecureDataFormat<AuthenticationTicket>>();
            kernel.Bind<ITextEncoder>().To<Base64UrlTextEncoder>();
            kernel.Bind<IDataSerializer<AuthenticationTicket>>().To<TicketSerializer>();
            kernel.Bind<IDataProtector>().ToMethod(context =>
                new DpapiDataProtectionProvider().Create("ASP.NET Identity"));

            #endregion

            #region Identity
            kernel.Bind<IUserStore<ApplicationUser, int>>().To<UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>>();
            kernel.Bind<IRoleStore<ApplicationRole, int>>().To<RoleStore<ApplicationRole, int, ApplicationUserRole>>();

            kernel.Bind<ApplicationUserManager>().ToSelf();
            kernel.Bind<ApplicationRoleManager>().ToSelf();
            kernel.Bind<ApplicationSignInManager>().ToSelf();
            kernel.Bind<IIdentityMessageService>().To(typeof(SmsService)).Named("Sms");
            kernel.Bind<IIdentityMessageService>().To(typeof(EmailService)).Named("Email");
            kernel.Bind<ApplicationRoleStore>().ToSelf();
            kernel.Bind<ApplicationUserStore>().ToSelf();

            #endregion

            #region HttpContextBase

            kernel.Bind<HttpContext>().ToMethod(ctx => HttpContext.Current).InRequestScope();
            kernel.Bind<HttpContextBase>().ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InRequestScope();
            kernel.Bind<IHttpContextBaseRepository>().To<HttpContextBaseRepository>();

            #endregion
        }        
    }
}
