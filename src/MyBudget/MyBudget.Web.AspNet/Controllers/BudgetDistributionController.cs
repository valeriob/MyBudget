using MyBudget.Commands;
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
            var budget = ProjectionManager.GetBudgetProjection(budgetId);

            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(budgetId);

            var model = new DistributionTimeViewModel(budgetId, budget, categories);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult SubmitCheckPoint(SubmitCheckPoint model)
        {
            model.Date = DateTime.Now; // TODO 
            try
            {
                var handler = CommandManager.Create<SubmitDistributionCheckPoint>();
                handler(model.ToCommand(GetCurrentUserId().ToString()));
            }
            catch 
            { 
            }
            return RedirectToAction(ByDistribution(model.BudgetId));
        }
    }

    public class DistributionTimeViewModel
    {
        public string BudgetId { get; private set; }
        public string BudgetName { get; private set; }

        public IEnumerable<CheckPointSlice> CheckPointsSlices { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }
        IEnumerable<CheckPoint> _checkPoints;

        public SubmitCheckPoint SubmitCheckPoint { get; private set; }
        public bool EnableCategories { get; set; }
        

        public DistributionTimeViewModel(string budgetId, IBudgetProjection budget, IEnumerable<Category> categories)
        {
            BudgetId = budgetId;
            BudgetName = budget.Name;
            Categories = categories;
            _checkPoints = budget.GetCheckPoints();

            var lines = budget.GetAllLines();
            CheckPointsSlices = Group(lines);

            var lastLine = lines.Select(s => s.Date).DefaultIfEmpty(DateTime.MinValue).Max();
            var currentSlide = CheckPointsSlices.First();

            var sharingGroups = currentSlide.Groups.Where(g => g.Name != null);

            decimal totalForEach = 0;
            if(sharingGroups.Any())
                totalForEach = sharingGroups.Select(r => r.TotalAmount).Sum() / sharingGroups.Count();

            SubmitCheckPoint = new SubmitCheckPoint
            {
                BudgetId = budgetId,
                CurrencyISOCode = budget.CurrencyISOCode,
                CheckPointId = "BudgetDistributionCheckPoints-" + Guid.NewGuid(),
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
                .Select(s => new CheckPointLines { Id = s.Id, Date = s.Date, Name = "non c'è !" })
                .Concat(new[] { new CheckPointLines { Date = DateTime.MaxValue } })
                .ToArray();

            int index = 0;
            var current = sch[index];

            foreach (var l in lines.OrderBy(d => d.Date))
            {
                //if (l.Date >= current.Date)
                //{
                //    index++;
                //    current = sch[index];
                //}
                //current.Lines.Add(l);
                if (l.Date < current.Date)
                {
                    current.Lines.Add(l);
                }
                else
                {
                    index++;
                    current = sch[index];
                    current.Lines.Add(l);
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
        public string CurrencyISOCode { get; set; }
        public string CheckPointId { get; set; }
        public DateTime Date { get; set; }
        public List<KeyAmount> Amounts { get; set; }


        internal SubmitDistributionCheckPoint ToCommand(string userId)
        {
            return new SubmitDistributionCheckPoint
            {
                UserId = userId,
                Date = Date,
                BudgetId = BudgetId,
                CheckPointId = CheckPointId,
                Amounts = Amounts.Select(s => new MyBudget.Domain.Budgets.DistributionKeyAmount 
                {
                      DistributionKey = s.DistributionKey,
                      Amount = new Amount(Currencies.Parse(CurrencyISOCode), s.Amount)
                }).ToArray()
            };
        }
    }
}