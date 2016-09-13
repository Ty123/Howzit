using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Howzit.API.Models;
using Howzit.API.Providers;
using Howzit.API.Results;
using Howzit.Domains.Contracts;
using Howzit.Domains.Models;
using Howzit.Domains.Enums;
using System.Web.Http.Results;
using System.Net;
using System.Linq;
using Howzit.DAL.Services;

namespace Howzit.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private IUnitOfWork unitOfWork;
        //private Logging logging = new Logging();

        public AccountController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<UserInfoViewModel> GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            // logging row action;
            unitOfWork.LogRepository.Add(new UserLog(string.Format("Get user name=[{0}] external info", externalLogin.UserName), null, Log.INFO, await unitOfWork.CurrentUser, unitOfWork.ApplicationUserManager.FindByName(externalLogin.UserName)));

            unitOfWork.Commit();

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public async Task<IHttpActionResult> Logout()
        {
            // logging use action
            var currentUser = await unitOfWork.CurrentUser;

            unitOfWork.LogRepository.Add(new UserLog("User id=[" + currentUser.Id + "] logout!", null, Log.INFO, currentUser, currentUser));
            unitOfWork.Commit();

            unitOfWork.AuthenticationManager.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            UserLog log = null;
            var actionLogger = await unitOfWork.CurrentUser;
            ApplicationUser user = await unitOfWork.ApplicationUserManager.FindByIdAsync(User.Identity.GetUserId<int>());

            if (user == null)
            {
                log = new UserLog("Invalid user id!", "No user found", Log.ERROR, actionLogger, null);
                unitOfWork.LogRepository.Add(log);
                unitOfWork.Commit();

                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (ApplicationUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            var logInfo = new UserLog("Retrieve user logins", null, Log.INFO, user, user);
            unitOfWork.LogRepository.Add(logInfo);
            unitOfWork.Commit();

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;
            var userId = User.Identity.GetUserId<int>();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join(" | ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", messages, Log.ERROR, actionLogger, unitOfWork.UserRepository.FindById(userId)));
                unitOfWork.Commit();

                return BadRequest(messages);
            }

            IdentityResult result = await unitOfWork.ApplicationUserManager.ChangePasswordAsync(userId, model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (string error in result.Errors)
                {
                    errors = string.Concat(errors, "|" + error);
                }

                unitOfWork.LogRepository.Add(new UserLog(string.Format("Unable to change user id=[{0}] password", userId), errors, Log.ERROR, actionLogger, unitOfWork.ApplicationUserManager.FindById(userId)));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            unitOfWork.LogRepository.Add(new UserLog(string.Format("Change user id=[{0}] password", userId), null, Log.UPDATE, actionLogger, unitOfWork.ApplicationUserManager.FindById(userId)));

            unitOfWork.Commit();

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;
            var user = unitOfWork.ApplicationUserManager.FindByName(model.UserName);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join(" | ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", messages, Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest(ModelState);
            }

            IdentityResult result = await unitOfWork.ApplicationUserManager.AddPasswordAsync(user.Id, model.NewPassword);

            if (!result.Succeeded)
            {
                // logging errors
                var errors = string.Join("|", result.Errors);

                unitOfWork.LogRepository.Add(new UserLog(string.Format("Unable to set password to user id=[{0}]", user.Id), errors, Log.ERROR, actionLogger, user));

                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            // logging row action
            unitOfWork.LogRepository.Add(new UserLog(string.Format("Set password for actionLogger id:: [{0}]", user.Id), null, Log.UPDATE, actionLogger, user)); ;
            unitOfWork.Commit();

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;
            var user = unitOfWork.ApplicationUserManager.FindByName(actionLogger.UserName);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join(" | ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", messages, Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest(ModelState);
            }

            unitOfWork.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = unitOfWork.AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                unitOfWork.LogRepository.Add(new UserLog("External login failure.", "External login failure.", Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("External login failure.", "The external login is already associated with an account.", Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await unitOfWork.ApplicationUserManager.AddLoginAsync(user.Id, new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                var errors = string.Join("|", result.Errors);
                unitOfWork.LogRepository.Add(new UserLog("Failure Add External Login!", errors, Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            unitOfWork.LogRepository.Add(new UserLog("Add external login user", null, Log.UPDATE, actionLogger, user));
            unitOfWork.Commit();

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            var actionLogger = await unitOfWork.CurrentUser;
            var user = unitOfWork.ApplicationUserManager.FindByName(actionLogger.UserName);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join(" | ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Invalid model state!", messages, Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await unitOfWork.ApplicationUserManager.RemovePasswordAsync(user.Id);
            }
            else
            {
                result = await unitOfWork.ApplicationUserManager.RemoveLoginAsync(user.Id,
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                var errors = string.Join("|", result.Errors);

                unitOfWork.LogRepository.Add(new UserLog("Unable to user login", errors, Log.ERROR, actionLogger, user));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            unitOfWork.LogRepository.Add(new UserLog("Remove user login", null, Log.UPDATE, actionLogger, user));
            unitOfWork.Commit();

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                unitOfWork.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await unitOfWork.ApplicationUserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                unitOfWork.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync((ApplicationUserManager)unitOfWork.ApplicationUserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync((ApplicationUserManager)unitOfWork.ApplicationUserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                unitOfWork.AuthenticationManager.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                unitOfWork.AuthenticationManager.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = unitOfWork.AuthenticationManager.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join("  ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Register with invalid model state!", messages, Log.ERROR, null, null));
                unitOfWork.Commit();

                unitOfWork.Commit();

                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await unitOfWork.ApplicationUserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors);

                unitOfWork.LogRepository.Add(new UserLog("Unable to register!", errors, Log.ERROR, null, null));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            unitOfWork.LogRepository.Add(new UserLog("Register new user!", null, Log.CREATE, null, null));
            unitOfWork.Commit();

            return new StatusCodeResult(HttpStatusCode.Created, Request);
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToArray();
                var messages = string.Join(" | ", errors);

                unitOfWork.LogRepository.Add(new UserLog("Register with invalid model state!", messages, Log.ERROR, null, null));
                unitOfWork.Commit();

                return BadRequest(ModelState);
            }

            var info = await unitOfWork.AuthenticationManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                unitOfWork.LogRepository.Add(new UserLog("Unable to get external login info!", "Unable to get info of " + model.Email, Log.ERROR, null, null));
                unitOfWork.Commit();

                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await unitOfWork.ApplicationUserManager.CreateAsync(user);
            var actionLogger = await unitOfWork.CurrentUser;

            if (!result.Succeeded)
            {
                var errors = string.Join("|", result.Errors);

                unitOfWork.LogRepository.Add(new UserLog("Unable to register external!", errors, Log.ERROR, null, null));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            result = await unitOfWork.ApplicationUserManager.AddLoginAsync(user.Id, info.Login);

            if (!result.Succeeded)
            {
                var errors = string.Join("|", result.Errors);

                unitOfWork.LogRepository.Add(new UserLog("Unable to register external!", errors, Log.ERROR, null, null));
                unitOfWork.Commit();

                return GetErrorResult(result);
            }

            unitOfWork.LogRepository.Add(new UserLog("Regiser external login email=[" + model.Email + "]", null, Log.ERROR, null, null));
            unitOfWork.Commit();

            return Ok();
        }

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No modelStates modelStates are available _taskOwner send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
