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
        UsersListProjection _users;
        UserCredentials _credentials;


        public ProjectionManager(IEventStoreConnection connection, UserCredentials credentials)
        {
            _connection = connection;
            _credentials = credentials;
            _users = new UsersListProjection(connection, credentials);
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
