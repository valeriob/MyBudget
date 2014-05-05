using MyBudget.Domain.ValueObjects;
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
        public IEnumerable<Currency> Currencies { get; set; }
        public string CurrencyISOCode { get; set; }


        public IEnumerable<System.Web.Mvc.SelectListItem> GetCurrencies()
        {
            return Currencies.Select(c => new System.Web.Mvc.SelectListItem
            {
                Value = c.IsoCode,
                Text = c.Name,
                Selected = string.Compare(c.IsoCode, CurrencyISOCode, true) == 0
            }).ToList();
        }
    }


    public class BudgetDetailsViewModel
    {
        public Budget Budget { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<string> DistributionKeys { get; set; }
 
    }
}