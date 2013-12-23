using EventStore.ClientAPI;
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
        UsersListProjection _users;

        public ProjectionManager(IEventStoreConnection connection)
        {
            _connection = connection;

            _users = new UsersListProjection(connection);
        }

        public void Run()
        {
            _users.Start();
        }

        public UsersListProjection GetUsersList()
        {
            return _users;
        }

    }
}
