using MyBudget.Domain.Lines;
using MyBudget.Infrastructure;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBudget.Web.AspNet.Models
{
    public class BudgetLinesViewModel
    {
        public IEnumerable<BudgetLine> Lines { get; private set; }
        public string BudgetId { get; private set; }

        public BudgetLinesViewModel(string budgetId, IEnumerable<BudgetLine> lines)
        {
            BudgetId = budgetId;
            Lines = lines;
        }
    }

    public class CreateBudgetLineViewModel
    {
        public string LineId { get; set; }
        public string BudgetId { get; set; }

        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyISOCode { get; set; }
    }

    public class EditBudgetLineViewModel
    {
        public string LineId { get; set; }
        public string BudgetId { get; set; }

        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyISOCode { get; set; }

        public IEnumerable<string> Categories { get; set; }
        public string BudgetName { get; set; }

        [Obsolete("Serialization", true)]
        public EditBudgetLineViewModel()
        {

        }

        public EditBudgetLineViewModel(string budgetName, IEnumerable<dynamic> events, IEnumerable<string> categories)
        {
            EventHelper.Apply(events, this);
            Categories = categories.ToList();
            BudgetName = budgetName;
        }



        public void When(LineCreated evnt)
        {
            LineId = evnt.LineId.ToString();
            BudgetId = evnt.BudgetId.ToString();
            Date = evnt.Date;
            Category = evnt.Category;
            Description = evnt.Description;
            Amount = evnt.Amount;
            CurrencyISOCode = evnt.Amount.GetCurrency().IsoCode;
        }

        public void When(LineExpenseChanged evnt)
        {
            Date = evnt.Date;
            Category = evnt.Category;
            Description = evnt.Description;
            Amount = evnt.Amount;
            CurrencyISOCode = evnt.Amount.GetCurrency().IsoCode;
        }
    }

}