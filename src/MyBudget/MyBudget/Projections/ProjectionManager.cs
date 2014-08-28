using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Infrastructure;
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
        IPEndPoint _endpoint;
        ApplicationUserProjection _applicationUsers;
        UserCredentials _credentials;
        UsersListProjection _users;
        BudgetsListProjection _budgets;
        CategoriesProjection _categories;
        Dictionary<string, BudgetLinesProjection> _budgetLines;
        Dictionary<string, BudgetProjection> _budget;
        IAdaptEvents _adapter;


        public ProjectionManager(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter) 
        {
            _adapter = adapter;
            _endpoint = endpoint;
            _credentials = credentials;
            _budgetLines = new Dictionary<string, BudgetLinesProjection>();
            _budget = new Dictionary<string, BudgetProjection>();
        }

        public void Run()
        {
            var usersStream = "categoria_Users";
            var budgetsStream = "categoria_Budgets";
            var categoriesStream = "categoria_Category";

            _applicationUsers = new ApplicationUserProjection(_endpoint, _credentials, _adapter, usersStream);
            _users = new UsersListProjection(_endpoint, _credentials, _adapter, usersStream);
            _budgets = new BudgetsListProjection(_endpoint, _credentials, _adapter, budgetsStream);
            _categories = new CategoriesProjection(_endpoint, _credentials, _adapter, categoriesStream);

            _applicationUsers.Start();
            _users.Start();
            _budgets.Start();
            _categories.Start();
        }


        public IEnumerable<dynamic> GetStreamEvents(string streamId)
        {
            return _adapter.GetStreamEvents(streamId);
        }

        public IUsersListProjection GetUsersList()
        {
            return _users;
        }

        public IApplicationUsersProjection GetApplicationUsers()
        {
            return _applicationUsers;
        }

        public IBudgetsListProjection GetBudgetsList()
        {
            return _budgets;
        }

        public ICategoriesProjection GetCategories()
        {
            return _categories;
        }

        public IBudgetLinesProjection GetBudgetLinesProjection(string budgetId)
        {
            BudgetLinesProjection blp = null;

            if (_budgetLines.TryGetValue(budgetId, out blp) == false)
            {
                var linesStream = "lines_of_" + budgetId;
                //_budgetLines[budgetId] = blp = new BudgetLinesProjection(budgetId, _endpoint, _credentials, _adapter, linesStream);
                _budgetLines[budgetId] = blp = new BudgetLinesProjection(budgetId, _endpoint, _credentials, _adapter);
                blp.Start();
            }
            return blp;
        }

        public IBudgetProjection GetBudgetProjection(string budgetId)
        {
            BudgetProjection blp = null;

            if (_budget.TryGetValue(budgetId, out blp) == false)
            {
                var linesStream = "stuff_of_" + budgetId;
                var b = GetBudgetsList().GetBudgetById(new Domain.Budgets.BudgetId(budgetId));

                _budget[budgetId] = blp = new BudgetProjection(b, _endpoint, _credentials, _adapter, linesStream);
                blp.Start();
            }
            return blp;
        }
    }

}
