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
        CategoriesProjection _categories;
        Dictionary<string, BudgetLinesProjection> _budgetLines;


        ProjectionManager(UserCredentials credentials)
        {
            _credentials = credentials;
          
            _budgetLines = new Dictionary<string, BudgetLinesProjection>();
        }
        public ProjectionManager(IPEndPoint endpoint, UserCredentials credentials) 
            : this(credentials)
        {
            _endpoint = endpoint;
        }
        public ProjectionManager(IEventStoreConnection connection, UserCredentials credentials)
            : this(credentials)
        {
            _connection = connection;
        }

        public void Run()
        {
            _users = new UsersListProjection(_endpoint, _credentials, null);
            _budgets = new BudgetsListProjection(_endpoint, _credentials, null);
            _categories = new CategoriesProjection(_endpoint, _credentials, null);
            //_users = new UsersListProjection(_endpoint, _credentials, "$category-Users");
            //_budgets = new BudgetsListProjection(_endpoint, _credentials, "$category-Budgets");

            _users.Start();
            _budgets.Start();
            _categories.Start();
        }

        public UsersListProjection GetUsersList()
        {
            return _users;
        }

        public BudgetsListProjection GetBudgetsList()
        {
            return _budgets;
        }
        public CategoriesProjection GetCategories()
        {
            return _categories;
        }

        public BudgetLinesProjection GetBudgetLinesProjection(string budgetId)
        {
            BudgetLinesProjection blp = null;

            if (_budgetLines.TryGetValue(budgetId, out blp) == false)
            {
                _budgetLines[budgetId] = blp = new BudgetLinesProjection(budgetId, _endpoint, _credentials);
                blp.Start();
            }
            return blp;
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
