using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class BudgetStatsController : MyBudgetController
    {

        public virtual ActionResult Index(string id)
        {
            //return RedirectToAction(Actions.ByCategory(id, null, null));
            ViewBag.BudgetId = id;
            return View();
        }

        public virtual ActionResult ByCategory(string budgetId, string From, string To)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (From != null)
                from = DateTime.Parse(From);
                //from = From;
            if (To != null)
                to = DateTime.Parse(To);
                //to = To;

            var projection = ProjectionManager.GetBudgetLinesProjection(budgetId);
            IEnumerable<BudgetLine> lines = projection.GetAllLinesBetween(from, to);
            if (from == null)
                from = lines.Select(s=>s.Date).DefaultIfEmpty(DateTime.MinValue).Min(r => r.Date.Date);
            if (to == null)
                to = lines.Select(s=>s.Date).DefaultIfEmpty(DateTime.MaxValue).Max(r => r.Date.Date);

            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(budgetId);
            var model = new BudgetStatsByCategoryViewModel(categories, lines, budgetId, "NA", from, to);

            return View(model);
        }

        public virtual ActionResult ByCategoryInTime(string budgetId, string From, string To, string GroupBy)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (From != null)
                from = DateTime.Parse(From);
            //from = From;
            if (To != null)
                to = DateTime.Parse(To);
            //to = To;

            GroupBy groupBy = MyBudget.Web.AspNet.Controllers.GroupBy.Year;

            try
            {
                groupBy = (GroupBy)Enum.Parse(typeof(GroupBy), GroupBy);
            }
            catch { }

            var projection = ProjectionManager.GetBudgetLinesProjection(budgetId);
            IEnumerable<BudgetLine> lines = projection.GetAllLinesBetween(from, to);
            if (from == null)
                from = lines.Select(s => s.Date).DefaultIfEmpty(DateTime.MinValue).Min(r => r.Date.Date);
            if (to == null)
                to = lines.Select(s => s.Date).DefaultIfEmpty(DateTime.MaxValue).Max(r => r.Date.Date);
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(budgetId);
            var model = new BudgetStatsByCategoryInTimeViewModel(categories, lines, budgetId, "NA", from, to, groupBy);

            return View(model);
        }

    }

    public class BudgetStatsByCategoryInTimeViewModel
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public IEnumerable<TimeGroup> TimeSerie { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }
        public  GroupBy Grouping { get; private set; }


        public BudgetStatsByCategoryInTimeViewModel(IEnumerable<Category> categories, IEnumerable<BudgetLine> lines,
            string budgetId, string budgetName, DateTime? from, DateTime? to, GroupBy grouping)
        {
            BudgetId = budgetId;
            BudgetName = budgetName;
            From = from;
            To = to;
            Categories = categories;// lines.Select(s => s.Category).Distinct().ToArray();
            Grouping = grouping;
            TimeSerie = Group(lines, grouping);
        }


        IEnumerable<TimeGroup> Group(IEnumerable<Projections.BudgetLine> lines, GroupBy grouping)
        {
            switch(grouping)
            {
                case  GroupBy.Year:
                    return lines.GroupBy(g => g.Date.Year)
                        .Select(s => new TimeGroup(s.Key + "", s.ToCategoryStats())).ToList();

                case GroupBy.Month:
                    return lines.GroupBy(g => new { g.Date.Year, g.Date.Month })
                        .Select(s => new TimeGroup(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[s.Key.Month - 1] + " " + s.Key.Year , s.ToCategoryStats())).ToList();

                case GroupBy.Week:
                    return lines.GroupBy(g => new { g.Date.Year, Week = GetIso8601WeekOfYear(g.Date) })
                        .Select(s => new TimeGroup(s.Key.Year % 100 +" #"+s.Key.Week , s.ToCategoryStats())).ToList();
                case GroupBy.Day:
                    return lines.GroupBy(g => g.Date)
                      .Select(s => new TimeGroup(s.Key.ToString("d"), s.ToCategoryStats())).ToList();
                default : 
                    throw new NotImplementedException();
            }
           
        }

        static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public string GetFormattedFrom()
        {
            if (From == null)
                return "";
            return From.Value.ToString("d");
        }

        public string GetFormattedTo()
        {
            if (To == null)
                return "";
            return To.Value.ToString("d");
        }

        public IEnumerable<GroupBy> GetGroupings()
        {
            yield return GroupBy.Year;
            yield return GroupBy.Month;
            yield return GroupBy.Week;
            yield return GroupBy.Day;
        }

        public string GetIsSelected(GroupBy g)
        {
            return g == Grouping ? "selected='selected'" : "";
        }
    }

    public static class Extensions
    {
        public static IEnumerable<CategoryStats> ToCategoryStats(this IEnumerable<Projections.BudgetLine> lines)
        {
            return lines.GroupBy(g => g.Category).Select(s => new CategoryStats
                {
                    Name = s.Key,
                    Amount = s.Select(r => r.Amount).Sum(),
                }).ToList();
        }

    }

    public enum GroupBy { Year, Month, Week, Day }

    public class TimeGroup
    {
        public string Group { get; private set; }
        public Amount TotalAmount { get; private set; }

        Dictionary<string, Amount> _categories;

        public TimeGroup(string group, IEnumerable<CategoryStats> categories)
        {
            Group = group;
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

    public class BudgetStatsByCategoryViewModel
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public IEnumerable<CategoryStats> Categories { get; private set; }
        public Amount TotalAmount { get; private set; }

        Dictionary<string,Category> _categories;
        public BudgetStatsByCategoryViewModel(IEnumerable<Category> categories, IEnumerable<BudgetLine> lines, string budgetId, string budgetName, DateTime? from, DateTime? to)
        {
            BudgetId = budgetId;
            BudgetName = budgetName;
            From = from;
            To = to;
            _categories = categories.ToDictionary(d => d.Id);

            Categories = lines.GroupBy(g => g.Category)
                .Select(s => new CategoryStats
                {
                    Id = s.Key,
                    Name = _categories[s.Key].Name,
                    Amount = s.Select(r => r.Amount).Sum(),
                }).OrderByDescending(d=> d.Amount)
                .ToList();

            TotalAmount = Categories.Select(s => s.Amount).Sum();
        }

        public string GetFormattedFrom()
        {
            if (From == null)
                return "";
            return From.Value.ToString("d");
        }

        public string GetFormattedTo()
        {
            if (To == null)
                return "";
            return To.Value.ToString("d");
        }

        public string CategoryName(string categoryId)
        {
            return _categories[categoryId].Name;
        }
    }


    public class CategoryStats
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Amount Amount { get; set; }
    }

}