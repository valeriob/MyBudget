using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using MyBudget.Web.AspNet.Models;

namespace MyBudget.Web.AspNet.Controllers
{
    public class MyUserManager
    {
        class UserMapping
        {
            public string UserId { get; set; }
            public UserLoginInfo ExternalId { get; set; }
        }

        static List<UserMapping> _users = new List<UserMapping>();

        public async Task<ApplicationUser> FindAsync(UserLoginInfo userLoginInfo)
        {
            var users = MvcApplication.ProjectionManager.GetUsersList();
            var map = users.FindByLogin(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey);

            //var map = _users.SingleOrDefault(r => r.ExternalId == userLoginInfo);
            if (map == null)
                return null;
            return new ApplicationUser
            {
                Id = map.Id,
                UserName = map.Id,
            };
            //return new ApplicationUser 
            //{ 
            //     Id = map.UserId,
            //     UserName = map.UserId,
            //};
        }

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo userLoginInfo)
        {
            var h = MvcApplication.CommandManager.Create<MyBudget.Commands.AddUser>();
            h.Handle(new MyBudget.Commands.AddUser());

            //_users.Add(new UserMapping { UserId = userId, ExternalId = userLoginInfo });
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return IdentityResult.Success;
        }

        public async Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string cookies)
        {
            var t = new System.Security.Principal.GenericIdentity(user.Id, cookies);

            var c2 = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Guid.NewGuid() + "");
            var c1 = new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity");

            return new ClaimsIdentity(t, new []{c1, c2});
        }

        internal void Create(ApplicationUser user, UserLoginInfo userLoginInfo)
        {
            var h = MvcApplication.CommandManager.Create<MyBudget.Commands.AddUser>();
            h.Handle(new MyBudget.Commands.AddUser 
            {
                UserId = user.Id,
                UserLoginInfo = new Budgets.ValueObjects.UserLoginInfo(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey)
            });
        }
    }

    [Authorize]
    public class AccountController : Controller
    {
        public MyUserManager UserManager { get; private set; }

        public AccountController()
        {
            UserManager = new MyUserManager();
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            var externalIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            var displayName = externalIdentity.Name;
            var email = externalIdentity.FindFirstValue(ClaimTypes.Email);

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            ApplicationUser user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var uid = MyBudget.Budgets.UserId.CreateNew().ToString();

                var user = new ApplicationUser() { Id = uid, UserName = model.UserName };
                UserManager.Create(user, info.Login);
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);

                //var user = new ApplicationUser() { UserName = model.UserName };
                //IdentityResult result = await UserManager.CreateAsync(user);
                //if (result.Succeeded)
                //{
                //    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                //    if (result.Succeeded)
                //    {
                //        await SignInAsync(user, isPersistent: false);
                //        return RedirectToLocal(returnUrl);
                //    }
                //}
                //AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }




        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

      

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}