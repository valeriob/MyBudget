using EventStore.ClientAPI;
using MyBudget.Budgets;
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

        public UsersListProjection(IEventStoreConnection connection):base(connection)
        {
        }

        public override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch { }
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
    }
}
