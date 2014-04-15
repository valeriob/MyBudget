using Microsoft.AspNet.Identity;
using MyBudget.Commands;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MyBudget.Web.AspNet.Controllers
{

    public interface IUserManager : IApplicationUsersProjection, IDisposable
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> RemoveLoginAsync(string userId, UserLoginInfo userLoginInfo);
        Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<IdentityResult> AddPasswordAsync(string userId, string newPassword);
        Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo userLoginInfo);
        Task<IdentityResult> CreateAsync(ApplicationUser user);
        Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string applicationCookie);
    }

    public class MyBudgetUserManager : IUserManager
    {
        IApplicationUsersProjection _projection;
        CommandManager _commandManager;
        public MyBudgetUserManager(IApplicationUsersProjection projection, CommandManager commandManager)
        {
            _projection = projection;
            _commandManager = commandManager;
        }


        public Task<ApplicationUser> FindAsync(string username, string password)
        {
            return _projection.FindAsync(username, password);
        }
        public Task<ApplicationUser> FindAsync(Domain.ValueObjects.UserLoginInfo userLoginInfo)
        {
            return _projection.FindAsync(userLoginInfo);
        }
        public List<Domain.ValueObjects.UserLoginInfo> GetLogins(string userId)
        {
            return _projection.GetLogins(userId);
        }
        public ApplicationUser FindById(string userId)
        {
            return _projection.FindById(userId);
        }



        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            var h = MvcApplication.CommandManager.Create<AddUser>();
            return TryExecute(() => h(new AddUser
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = user.Id,
                UserName = user.UserName,
                Password = password,
            }));
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            var h = MvcApplication.CommandManager.Create<AddUser>();
            return TryExecute(() => h(new AddUser
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = user.Id,
                UserName = user.UserName,
            }));
        }

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo userLoginInfo)
        {
            var h = MvcApplication.CommandManager.Create<AddUserLogin>();
            return TryExecute(() => h(new AddUserLogin
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = userId,
                UserLoginInfo = new Domain.ValueObjects.UserLoginInfo(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey)
            }));
        }
        public async Task<IdentityResult> RemoveLoginAsync(string userId, UserLoginInfo userLoginInfo)
        {
            var h = MvcApplication.CommandManager.Create<RemoveUserLogin>();
            return TryExecute(() => h(new RemoveUserLogin
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = userId,
                UserLoginInfo = new Domain.ValueObjects.UserLoginInfo(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey)
            }));
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var h = MvcApplication.CommandManager.Create<ChangeUserPassword>();
            return TryExecute(() => h(new ChangeUserPassword
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = userId,
                OldPassword = oldPassword,
                NewPassword = newPassword
            }));
        }

        public async Task<IdentityResult> AddPasswordAsync(string userId, string newPassword)
        {
            var h = MvcApplication.CommandManager.Create<AddUserPassword>();
            return TryExecute(() => h(new AddUserPassword
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                UserId = userId,
                Password = newPassword
            }));
        }



        public async Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string applicationCookie)
        {
            var t = new System.Security.Principal.GenericIdentity(user.UserName, applicationCookie);
            var c2 = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.Id + "");
            var c1 = new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity");

            return new ClaimsIdentity(t, new[] { c1, c2 });
        }

        public void Dispose()
        {

        }


        IdentityResult TryExecute(Action action)
        {
            try
            {
                action();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return new IdentityResult(ex.Message);
            }
        }

    }

}