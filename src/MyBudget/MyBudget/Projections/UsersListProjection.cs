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
    public interface IUsersListProjection
    {
        User FindByLogin(string provider, string key);
        User FindById(string userId);
        Task<User> FindByIdAsync(string userId);
    }

    public class UsersListProjection : InMemoryProjection, IUsersListProjection
    {
        Dictionary<string, User> _users;


        public UsersListProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
            : base(endpoint, credentials, adapter, streamName)
        {
            _users = new Dictionary<string, User>();
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
            var s = new User();
            s.Apply(evnt);
            _users.Add(evnt.UserId.ToString(), s);
        }


        public User FindByLogin(string provider, string key)
        {
            var users = _users.Values.Where(r => r.Is(provider, key));
            return users.FirstOrDefault();
        }

        public User FindById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            User user = null;
            _users.TryGetValue(userId, out user);
            return user;
        }

        public async Task<User> FindByIdAsync(string userId)
        {
            while (HasLoaded == false)
                await Task.Delay(100);
            return _users[userId];
        }
    }

    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public UserLoginInfo LoginInfo { get; set; }


        public void Apply(UserCreated evnt)
        {
            Id = evnt.UserId.ToString();
            UserName = evnt.UserName;
            Created = evnt.Timestamp;
            LoginInfo = evnt.LoginInfo;
        }

        public bool Is(string provider, string key)
        {
            return LoginInfo.LoginProvider == provider && LoginInfo.ProviderKey == key;
        }
    }
}
