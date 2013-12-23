using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class ProjectionManager
    {
        IEventStoreConnection _connection;
        UserCredentials _credentials;
        UsersListProjection _users;
        BudgetsListProjection _budgets;
        

        public ProjectionManager(IEventStoreConnection connection, UserCredentials credentials)
        {
            _connection = connection;
            _credentials = credentials;
            _users = new UsersListProjection(connection, credentials);
            _budgets = new BudgetsListProjection(connection, credentials);
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
        public override void Dispatch(dynamic evnt)
        {
            count++;
        }
    }

}
