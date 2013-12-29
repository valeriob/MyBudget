using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class BudgetStatsController : MyBudgetController
    {
        public virtual ActionResult Index(string id)
        {
            return RedirectToAction(Actions.ByCategory(id, null, null));
        }

        public virtual ActionResult ByCategory(string budgetId, DateTime? from, DateTime? to)
        {
            var projection = ProjectionManager.GetBudgetLinesProjection(budgetId);
            var lines = projection.GetAllLinesBetween(from, to);
            var model = new BudgetStatsByCategoryViewModel(lines, budgetId, "NA", from, to);

            return View(model);
        }

	}

    public class BudgetStatsByCategoryViewModel
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public IEnumerable<CategoryStats> Categories { get; private set; }

        public BudgetStatsByCategoryViewModel(IEnumerable<Projections.BudgetLine> lines, string budgetId, string budgetName, DateTime? from, DateTime? to)
        {
            BudgetId = budgetId;
            BudgetName = budgetName;
            From = from;
            To = to;

            Categories = lines.GroupBy(g => g.Category)
                .Select(s => new CategoryStats 
                { 
                    Category = s.Key,
                    Amount = s.Select(r=> r.Amount).Sum(),
                }).ToList();
        }


    }

    public class CategoryStats
    {
        public string Category { get; set; }
        public Amount Amount { get; set; }
    }

}