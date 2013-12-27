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
        public EditBudgetLineViewModel(string budgetId, BudgetLine line)
        {
            LineId = line.Id;
            BudgetId = budgetId;
            Date = line.Date;
            Category = line.Category;
            Description = line.Description;
            //Amount = line.Amount
            //CurrencyISOCode = line.Amount.GetHashCode
        }
        public string LineId { get; set; }
        public string BudgetId { get; set; }

        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyISOCode { get; set; }
    }

}