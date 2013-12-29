﻿using EventStore.ClientAPI;
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
        IEventStoreConnection _connection;
        IPEndPoint _endpoint;
        UserCredentials _credentials;
        UsersListProjection _users;
        BudgetsListProjection _budgets;
        CategoriesProjection _categories;
        Dictionary<string, BudgetLinesProjection> _budgetLines;
        IAdaptEvents _adapter;


        public ProjectionManager(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter) 
        {
            _adapter = adapter;
            _endpoint = endpoint;
            _credentials = credentials;
            _budgetLines = new Dictionary<string, BudgetLinesProjection>();
        }

        public void Run()
        {
            var usersStream = "categoria_Users";
            var budgetsStream = "categoria_Budgets";
            //var categoriesStream = "categoria_Budgets";
            _users = new UsersListProjection(_endpoint, _credentials, _adapter, usersStream);
            _budgets = new BudgetsListProjection(_endpoint, _credentials, _adapter, budgetsStream);
            _categories = new CategoriesProjection(_endpoint, _credentials, _adapter, null);
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
                var linesStream = "lines_of_" + budgetId;
                _budgetLines[budgetId] = blp = new BudgetLinesProjection(budgetId, _endpoint, _credentials, _adapter, linesStream);
                blp.Start();
            }
            return blp;
        }


        public IEnumerable<dynamic> GetStreamEvents(string streamId)
        {
            return _adapter.GetStreamEvents(streamId);
        }
    }

   


}
