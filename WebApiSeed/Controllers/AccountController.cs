using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using WebApiSeed.Models;
using WebApiSeed.Providers;
using WebApiSeed.Results;
using WebApiSeed.AxHelpers;
using WebApiSeed.DataAccess.Repositories;
using System.Linq;
using WebGrease.Css.Extensions;
using WebApiSeed.DataAccess.Filters;

namespace WebApiSeed.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        private readonly UserRepository _userRepo = new UserRepository();


        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }
        public AccountController()
            : this(new UserManager<User>(new UserStore<User>(new AppDbContext())))
        {
        }

        public AccountController(UserManager<User> userManager)
        {
            UserManager = userManager;
            UserManager.UserValidator = new UserValidator<User>(UserManager)
            {
                AllowOnlyAlphanumericUserNames =
                    false
            };
        }

        public AccountController(UserManager<User> userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public UserManager<User> UserManager { get; private set; }



        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin?.LoginProvider
            };
        }

        // GET security/signin
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultObj> Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid) throw new Exception("Please check the login details");

                var user = await UserManager.FindAsync(model.UserName, model.Password);

                if (user == null) throw new Exception("Invalid Username or Password");

                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

                var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
                var token = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);

                var data = new
                {
                    user.Id,
                    Username = user.UserName,
                    user.Name,
                    Role = new
                    {
                        user.Profile.Id,
                        user.Profile.Name,
                        Privileges = user.Profile.Privileges.Split(',')
                    },
                    token
                };

                return WebHelpers.BuildResponse(data, "Login Successfull", true, 0);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        // POST api/Account/Logout
        [HttpGet]
        [Route("Logout")]
        public ResultObj Logout()
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return WebHelpers.BuildResponse(new { }, "User Logged Out", true, 0);
        }

        [Route("GetUsers")]
        public ResultObj GetUsers()
        {
            try
            {
                var data = _userRepo.Get()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Email,
                        x.PhoneNumber,
                        x.UserName,
                        RoleId = x.ProfileId,
                        Role = new { x.Profile.Id, x.Profile.Name }
                    }).ToList();
                return WebHelpers.BuildResponse(data, "", true, data.Count);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        //[Authorize]
        [HttpGet]
        [Route("GetRoles")]
        public ResultObj GetRoles()
        {
            ResultObj results;
            try
            {
                var data = new AppDbContext().Roles.Select(x => x.Name).ToList();
                results = WebHelpers.BuildResponse(data, "", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        // POST api/Account/Register
        [Route("CreateUser")]
        public async Task<ResultObj> Register(RegisterBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var role = new ProfileRepository().Get(model.RoleId);

                var user = new User
                {
                    UserName = model.UserName,
                    ProfileId = role.Id,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);


                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                return WebHelpers.BuildResponse(user, "User Created Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [HttpPut]
        public async Task<ResultObj> UpdateUser(UpdateUserModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var user = _userRepo.Get(model.UserName);
                var role = new ProfileRepository().Get(model.RoleId);

                if (user == null) return WebHelpers.BuildResponse(null, "Updating user not found. Please update an existing user", false, 0);
                var oldRoles = user.Profile.Privileges.Split(',');

                user.ProfileId = role.Id;
                user.Name = model.Name;
                user.UpdatedAt = DateTime.UtcNow;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                _userRepo.Update(user);

                //Remove old reles
                oldRoles.ForEach(x => UserManager.RemoveFromRole(user.Id, x));

                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                //Change Password
                if (!string.IsNullOrEmpty(model.Password))
                {
                    UserManager.RemovePassword(user.Id);
                    UserManager.AddPassword(user.Id, model.Password);
                }

                var data = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.PhoneNumber,
                    user.UserName,
                    RoleId = user.ProfileId,
                    Role = new { user.Profile.Id, user.Profile.Name },
                };

                return WebHelpers.BuildResponse(data, "User Created Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [HttpPut]
        [Route("UpdateProfile")]
        public ResultObj UpdateProfile(Profile profile)
        {
            ResultObj results;
            try
            {
                if (!IsValidProfile(profile))
                    return WebHelpers.BuildResponse(profile, "Role must have a name and at least one profile.", false, 1);
                var oldRoles = new BaseRepository<Profile>().Get(profile.Id).Privileges.Split(',');
                var newRoles = profile.Privileges.Split(',');

                var users = _userRepo.Get(new UserFilter { ProfileId = profile.Id });
                foreach (var user in users)
                {
                    UserManager.RemoveFromRoles(user.Id, oldRoles);
                    var user1 = user;
                    newRoles.ForEach(r => UserManager.AddToRole(user1.Id, r.Trim()));
                }

                new BaseRepository<Profile>().Update(profile);

                results = WebHelpers.BuildResponse(profile, "Role Update Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        private static bool IsValidProfile(Profile profile)
        {
            return !string.IsNullOrEmpty(profile.Privileges) && !string.IsNullOrEmpty(profile.Name);
        }


        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteUser")]
        public ResultObj DeleteUser(string id)
        {
            ResultObj results;
            try
            {
                _userRepo.Delete(id);
                results = WebHelpers.BuildResponse(id, "User Deleted Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [Route("Register")]
        [AllowAnonymous]
        public async Task<ResultObj> SignUp(RegisterBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var role = new ProfileRepository().Get(model.ProfileId);

                //Todo: Check Code Validation Somehow

                var user = new User
                {
                    UserName = model.UserName,
                    ProfileId = model.ProfileId,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);


                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                return WebHelpers.BuildResponse(null, "Registration Successful", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resetpassword")]
        public ResultObj ResetPassword(ResetModel rm)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var existing = db.ResetRequests.FirstOrDefault(x => x.Token == rm.Token && x.IsActive);
                    if (existing == null) throw new Exception("Password reset was not complete");

                    var us = db.Users.FirstOrDefault(x => x.UserName == existing.Email && !x.Hidden && !x.IsDeleted);
                    if (us == null) throw new Exception("System Error");
                    var result = UserManager.RemovePassword(us.Id);
                    if (result.Succeeded)
                    {
                        var res = UserManager.AddPassword(us.Id, rm.Password);
                        if (res.Succeeded) existing.IsActive = false;
                        else throw new Exception(string.Join(", ", res.Errors));
                    }
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(null, "Password Changed Successfully", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("reset")]
        public ResultObj Reset(string email)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var existing = db.Users.FirstOrDefault(x => x.UserName == email && !x.Hidden && !x.Locked);
                    if (existing == null) throw new Exception("Sorry email is not valid. Enjoy!!");
                    var newRecord = new ResetRequest
                    {
                        Email = email,
                        Token = MessageHelpers.GenerateRandomString(32),
                        Date = DateTime.Now,
                        Ip = Request.Headers.Referrer.AbsoluteUri,
                        IsActive = true
                    };
                    db.ResetRequests.Add(newRecord);

                    // create a password reset entry
                    var link = Request.Headers.Referrer.AbsoluteUri + "#/resetpassword/" + newRecord.Token;
                    var emailMsg = new EmailOutboxEntry
                    {
                        Message =
                            $"<h3>Password Reset Request</h3> <br/><br/> Please follow the link below to change your password. <br/><br/><b><a href='{link}'>Click here</a></b> to reset your password.<br/><br/><br/><br/>Please ignore this message if you did not make this request.<br/><br/>Thank you. <br/>",
                        Subject = "Password Reset",
                        Sender = "support@somedomain.com",
                        Receiver = newRecord.Email,
                        Created = DateTime.Now
                    };
                    db.EmailOutboxEntries.Add(emailMsg);
                    db.SaveChanges();
                    MessageHelpers.SendEmailMessage(emailMsg.Id);

                    return WebHelpers.BuildResponse(null, "Password reset link has been sent to your email.", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }


        // POST api/Account/ChangePassword
        [Authorize]
        [Route("ChangePassword")]
        public async Task<ResultObj> ChangePassword(ChangePasswordBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(),
                    model.OldPassword, model.NewPassword);

                return !result.Succeeded ? WebHelpers.ProcessException(result)
                    : WebHelpers.BuildResponse(model, "Password changed sucessfully.", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }


        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
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

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

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
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            User user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
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

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new User() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

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
                    // No ModelState errors are available to send, so just return an empty BadRequest.
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
