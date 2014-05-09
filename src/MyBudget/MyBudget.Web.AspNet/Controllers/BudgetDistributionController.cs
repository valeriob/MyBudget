using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using MyBudget.Projections;
using MyBudget.Web.AspNet.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class BudgetDistributionController : MyBudgetController
    {
        public virtual ActionResult ByDistribution(string budgetId)
        {
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(new MyBudget.Domain.Budgets.BudgetId(budgetId));
            var linesPrj = ProjectionManager.GetBudgetLinesProjection(budgetId);
            var lines = linesPrj.GetAllLines();

            var checkPoints = Enumerable.Empty<CheckPoint>();

            var cp = new SubmitCheckPoint();

            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(budgetId);
            var model = new DistributionTimeViewModel(categories, lines, checkPoints, budgetId, "NA");

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult SubmitCheckPoint(SubmitCheckPoint model)
        {
            return RedirectToAction(ByDistribution(model.BudgetId));
        }
    }

    public class DistributionTimeViewModel
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }

        public IEnumerable<CheckPointSlice> CheckPointsSlices { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }
        IEnumerable<CheckPoint> _checkPoints;

        public SubmitCheckPoint SubmitCheckPoint { get; private set; }
        public bool EnableCategories { get; set; }

        public DistributionTimeViewModel(IEnumerable<Category> categories, IEnumerable<BudgetLine> lines, IEnumerable<CheckPoint> checkPoints,
            string budgetId, string budgetName)
        {
            BudgetId = budgetId;
            BudgetName = budgetName;
            Categories = categories;
            _checkPoints = checkPoints;

            CheckPointsSlices = Group(lines);

            var lastLine = lines.Select(s=> s.Date).DefaultIfEmpty(DateTime.MinValue).Max();
            var currentSlide = CheckPointsSlices.First();

            var sharingGroups = currentSlide.Groups.Where(g => g.Name != null);

            var totalForEach = sharingGroups.Select(r => r.TotalAmount).Sum() / sharingGroups.Count();

            SubmitCheckPoint = new SubmitCheckPoint 
            {
                BudgetId = budgetId,
                CheckPointId = Guid.NewGuid() + "",
                Date = lastLine,
                Amounts = sharingGroups.Select(s => new MyBudget.Web.AspNet.Models.KeyAmount
                {
                    DistributionKey = s.Name,
                    Amount = totalForEach - s.TotalAmount,
                }).ToList(),
            };
        }


        IEnumerable<CheckPointSlice> Group(IEnumerable<Projections.BudgetLine> lines)
        {
            var sch = _checkPoints.OrderBy(d => d.Date)
                .Select(s => new CheckPointLines { Id = s.Id, Date = s.Date, Name = s.Name })
                .Concat(new[] { new CheckPointLines { Date = DateTime.MaxValue } })
                .ToArray();

            int index = 0;
            var current = sch[index];

            foreach (var l in lines.OrderBy(d => d.Date))
            {
                if (l.Date < current.Date)
                {
                    current.Lines.Add(l);
                }
                else
                {
                    index++;
                    current = sch[index];
                }
            }

            var grps = sch.Select(s => new CheckPointSlice
            {
                Id = s.Id,
                Date = s.Date,
                Name = s.Name,
                Groups = s.Lines.GroupBy(d => d.DistributionKey)
                    .Select(c => new DistributionGroups(c.Key,
                                    c.GroupBy(r => r.Category).Select(u => new IdNameAmount { Id = u.Key, Name = u.Key, Amount = u.Select(o => o.Amount).Sum() })))
                                    .OrderByDescending(d=> d.Name)
                                    .ToList(),
                TotalAmount = s.Lines.Select(r=> r.Amount).Sum(),
            });

            return grps.OrderByDescending(d=> d.Date).ToList();
        }


        class CheckPointLines
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public List<BudgetLine> Lines { get; set; }

            public CheckPointLines()
            {
                Lines = new List<BudgetLine>();
            }
        }

    }

    public class CheckPoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
   

    public class CheckPointSlice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Amount TotalAmount { get; set; }

        public IEnumerable<DistributionGroups> Groups { get; set; }

        public CheckPointSlice()
        {
            Groups = new List<DistributionGroups>();
        }

        
    }


    public class DistributionGroups
    {
        public string Name { get; private set; }
        public Amount TotalAmount { get; private set; }

        Dictionary<string, Amount> _categories;

        public DistributionGroups(string name, IEnumerable<IdNameAmount> categories)
        {
            Name = name;
            _categories = categories.ToDictionary(d => d.Name, d => d.Amount);

            TotalAmount = categories.Select(s => s.Amount).Sum();
        }

        public Amount OfCategory(string category)
        {
            Amount result = null;
            if (_categories.TryGetValue(category, out result))
                return result;

            return Amount.Zero(TotalAmount.GetCurrency());
        }
    }



    public class SubmitCheckPoint
    {
        public string BudgetId { get; set; }
        public string CheckPointId { get; set; }
        public DateTime Date { get; set; }
        public List<KeyAmount> Amounts { get; set; }

    }
}