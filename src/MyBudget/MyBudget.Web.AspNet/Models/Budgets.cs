using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBudget.Web.AspNet.Models
{
    public class BudgetListViewModel
    {
        public IEnumerable<Budget> Budgets { get; private set; }

        public BudgetListViewModel(IEnumerable<Budget> budgets)
        {
            Budgets = budgets;
        }

    
    }

    public class CreateBudgetViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    public class BudgetDetailsViewModel
    {
        public Budget Budget { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}