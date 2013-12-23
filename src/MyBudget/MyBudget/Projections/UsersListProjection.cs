using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class UsersListProjection : InMemoryProjection
    {
        Dictionary<string, UserState> _users = new Dictionary<string, UserState>();

        public UsersListProjection(IEventStoreConnection connection, UserCredentials credentials):base(connection, credentials)
        {
        }

        public override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }
        public void When(UserCreated evnt)
        {
            var s = new UserState();
            s.Apply(evnt);
            _users.Add(evnt.UserId.ToString(), s);
        }

        public UserState FindByLogin(string provider, string key)
        {
            var users = _users.Values.Where(r=> r.GetUserLoginInfo().LoginProvider == provider && r.GetUserLoginInfo().ProviderKey == key);
            return users.FirstOrDefault();
        }

        public UserState FindById(string userId)
        {
            return _users.Values.SingleOrDefault(r => r.Is(userId));
        }
    }
}
