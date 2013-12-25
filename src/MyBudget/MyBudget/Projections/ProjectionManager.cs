using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class ProjectionManager
    {
        IEventStoreConnection _connection;
        IPEndPoint _endpoint;
        UserCredentials _credentials;
        UsersListProjection _users;
        BudgetsListProjection _budgets;

        public ProjectionManager(IPEndPoint endpoint, UserCredentials credentials)
        {
            _endpoint = endpoint;
            _credentials = credentials;

            _users = new UsersListProjection(_endpoint, credentials);
            _budgets = new BudgetsListProjection(_endpoint, credentials);

        }
        public ProjectionManager(IEventStoreConnection connection, UserCredentials credentials)
        {
            _connection = connection;
            _credentials = credentials;
            _users = new UsersListProjection(_endpoint, credentials);
            _budgets = new BudgetsListProjection(_endpoint, credentials);
        }

        IEventStoreConnection CreateConnection()
        {
            return EventStoreConnection.Create(_endpoint);
        }

        public void Run()
        {
            _users.Start();
            _budgets.Start();
        }

        public UsersListProjection GetUsersList()
        {
            return _users;
        }

        public BudgetsListProjection GetBudgetsList()
        {
            return _budgets;
        }

    }

   
    public class CountAllEventsProjection : InMemoryProjection
    {
        int count;
        protected override void Dispatch(dynamic evnt)
        {
            count++;
        }
    }

}
