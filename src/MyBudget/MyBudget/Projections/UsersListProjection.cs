using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Users;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class UsersListProjection : InMemoryProjection
    {
        Dictionary<string, User> _users;

        public UsersListProjection(IEventStoreConnection connection, UserCredentials credentials):base(connection, credentials)
        {
            _users = new Dictionary<string, User>();
        }

        public UsersListProjection(IPEndPoint endpoint, UserCredentials credentials)
            : base(endpoint, credentials)
        {
            _users = new Dictionary<string, User>();
        }

        protected override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

        void When(UserCreated evnt)
        {
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
