using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Users;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public interface IApplicationUsersProjection
    {
        Task<ApplicationUser> FindAsync(string username, string password);
        Task<ApplicationUser> FindAsync(UserLoginInfo userLoginInfo);
        List<UserLoginInfo> GetLogins(string userId);
        ApplicationUser FindById(string userId);
    }

    public class ApplicationUserProjection : InMemoryProjection, IApplicationUsersProjection
    {
        Dictionary<string, ApplicationUser> _users;


        public ApplicationUserProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
            : base(endpoint, credentials, adapter, streamName)
        {
            _users = new Dictionary<string, ApplicationUser>();
        }

        protected override void Dispatch(dynamic evnt)
        {
            try
            {
                ((dynamic)this).When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

        void When(UserCreated evnt)
        {
            if (_users.ContainsKey(evnt.UserId.ToString()))
                return;
            var s = new ApplicationUser();
            s.Apply(evnt);
            _users.Add(evnt.UserId.ToString(), s);
        }
        void When(UserLoginAdded evnt)
        {
            _users[evnt.UserId.ToString()].Apply(evnt);
        }
        void When(UserLoginRemoved evnt)
        {
            _users[evnt.UserId.ToString()].Apply(evnt);
        }
        void When(UserPasswordChanged evnt)
        {
            _users[evnt.UserId.ToString()].Apply(evnt);
        }


        public async Task<ApplicationUser> FindAsync(string username, string password)
        {
            var user = _users.Values.SingleOrDefault(r => r.UserName == username);
            if (user == null)
                throw new Exception("User not found");
            if(user.PasswordMatches(password) == false)
                throw new Exception("User not found");

            return user;
        }

        public async Task<ApplicationUser> FindAsync(UserLoginInfo userLoginInfo)
        {
            return _users.Values.SingleOrDefault(d => d.CanLoginWith(userLoginInfo));
        }

        public List<UserLoginInfo> GetLogins(string userId)
        {
            return _users[userId].GetLogins();
        }

        public ApplicationUser FindById(string userId)
        {
            return _users[userId];
        }
    }

    public class ApplicationUser //: IdentityUser
    {
        public string UserName { get; set; }
        public string Id { get; set; }
        public string PasswordHash { get; set; }
        List<UserLoginInfo> _logins = new List<UserLoginInfo>();

        internal void Apply(UserCreated evnt)
        {
            Id = evnt.UserId.ToString();
            UserName = evnt.UserName;
        }
        internal void Apply(UserLoginAdded evnt)
        {
            _logins.Add(evnt.LoginInfo);
        }
        internal void Apply(UserLoginRemoved evnt)
        {
            _logins.Remove(evnt.LoginInfo);
        }
        internal void Apply(UserPasswordChanged evnt)
        {
            PasswordHash = evnt.Password;
        }


        internal bool PasswordMatches(string password)
        {
            return PasswordHash == password;
        }


        internal bool CanLoginWith(UserLoginInfo info)
        {
            return _logins.Contains(info);
        }

        internal List<UserLoginInfo> GetLogins()
        {
            return _logins;
        }
    }

}
