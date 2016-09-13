using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Howzit.API.Providers;
using Howzit.API.Models;
using System.Web.Http;
using Howzit.DAL.Context;
using Howzit.Domains.Contracts;
using Howzit.DAL.Repositories;
using Microsoft.Owin.Security.Facebook;
using System.Threading.Tasks;
using System.Security.Claims;
using Howzit.Domains.Models;

namespace Howzit.API
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and logUser manager _taskOwner use a single instance per request
            var db = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            var userManager = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ApplicationUserManager)) as ApplicationUserManager;

            userManager.SeedDatabase();

            app.CreatePerOwinContext(() => db);
            app.CreatePerOwinContext<ApplicationUserManager>(() => (ApplicationUserManager)userManager);

            // Enable the application _taskOwner use a cookie _taskOwner store information for the signed in logUser
            // and _taskOwner use a cookie _taskOwner temporarily store information about a logUser logError in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

            // Enable the application _taskOwner use bearer tokens _taskOwner authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines _taskOwner enable logError in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            var facebookProvider = new FacebookAuthenticationProvider()
            {
                OnAuthenticated = (context) =>
                {
                    // Add the email id to the claim
                    context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email));
                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };
            var options = new FacebookAuthenticationOptions()
            {
                AppId = "586269381544092",
                AppSecret = "8f583cff72145638c38310586ac1e395",
                Provider = facebookProvider
            };
            options.Scope.Add("email");
            app.UseFacebookAuthentication(options);

            //app.UseFacebookAuthentication(
            //    appId: "586269381544092",
            //    appSecret: "8f583cff72145638c38310586ac1e395");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}
