
using MyBudget.Domain.Budgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBudget.Web.AspNet.Models
{
    public class BudgetListViewModel
    {
        public IEnumerable<BudgetState> Budgets { get; private set; }

        public BudgetListViewModel(IEnumerable<BudgetState> budgets)
        {
            Budgets = budgets;
        }
    }

}