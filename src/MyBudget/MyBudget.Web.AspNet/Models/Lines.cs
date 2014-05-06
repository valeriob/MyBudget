using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public class BudgetLinesPagedViewModel
    {
        public IEnumerable<BudgetLine> Lines { get; private set; }
        public string BudgetId { get; private set; }
        public DateTime? From { get; private set; }
        public DateTime? To { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }
        public string Category { get; private set; }

        public BudgetLinesPagedViewModel(string budgetId, PagedResult<BudgetLine> lines, DateTime? from, DateTime? to, IEnumerable<Category> categories, string category)
        {
            From = from;
            To = to;
            BudgetId = budgetId;
            Lines = lines;
            PageIndex = lines.PageIndex;
            TotalPages = lines.TotalPages();

            var c = categories.ToList();
            c.Insert(0, new Category());

            Categories = c;
            Category = category;
        }

        public bool FirstLinkVisible()
        {
            return PageIndex != 0;
        }
        public bool PrevLinkVisible()
        {
            return PageIndex > 0;
        }
        public bool NextLinkVisible()
        {
            return PageIndex < TotalPages;
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

        public IEnumerable<System.Web.Mvc.SelectListItem> GetCategories()
        {
            return Categories.Select(s => new System.Web.Mvc.SelectListItem
            {
                Value = s.Id,
                Text = s.Name,
                Selected = s.Id == Category
            });
        }

        public string CategoryNameFor(BudgetLine line)
        {
            return Categories.Where(r => r.Id == line.Category).Select(s => s.Name).First();
        }
    }
    //public class BudgetLineViewModel
    //{
    //    public string Id { get; private set; }
    //    public DateTime Timestamp { get; private set; }
    //    public DateTime Date { get; private set; }
    //    public Amount Amount { get; private set; }
    //    public string Category { get; private set; }
    //    public string Description { get; private set; }
    //}

    public class EditBudgetLineViewModel
    {
        [Required]
        public string LineId { get; set; }
        [Required]
        public string BudgetId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Category { get; set; }
        public string DistributionKey { get; set; }
        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string CurrencyISOCode { get; set; }
        public string BudgetName { get; set; }

        IEnumerable<Category> _categories;
        IEnumerable<Currency> _currencies;
        IEnumerable<string> _keys;

        [Obsolete("Serialization", true)]
        public EditBudgetLineViewModel()
        {

        }

        public EditBudgetLineViewModel(string budgetName, string budgetId, string currencyISOCode, IEnumerable<Category> categories, IEnumerable<string> distributionKeys, IEnumerable<Currency> currencies)
        {
            BudgetName = budgetName;
            _categories = categories.ToList();
            _currencies = currencies;

            BudgetId = budgetId;
            LineId = MyBudget.Domain.Lines.LineId.Create(new MyBudget.Domain.Budgets.BudgetId(budgetId)).ToString();
            Date = DateTime.Now;
            CurrencyISOCode = currencyISOCode;
            _keys = distributionKeys;
        }

        public EditBudgetLineViewModel(string budgetName, string budgetId, string currencyISOCode, IEnumerable<dynamic> events, IEnumerable<Category> categories, IEnumerable<string> distributionKeys, IEnumerable<Currency> currencies)
            : this(budgetName, budgetId, currencyISOCode, categories, distributionKeys, currencies)
        {
            EventHelper.Apply(events, this);
        }


        internal void LoadUserInputFrom(EditBudgetLineViewModel model)
        {
            BudgetName = model.BudgetName;
            BudgetId = model.BudgetId;
            LineId = model.LineId;
            Date = model.Date;
            CurrencyISOCode = model.CurrencyISOCode;
        }

        /*
        public string GetJsonCategories()
        {
            var array = _categories.Select(c => new
            {
                id = c,
                name = c,
            }).ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(array);
        }
        public string GetJsonSelectedCategories()
        {
            var array = _categories.Where(c => string.Compare(c, Category, true) == 0).Select(c => new
            {
                id = c,
                name = c,
            }).ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(array);
        }


        public string GetJsonCurrencies()
        {
            var array = _currencies.Select(c => new
            {
                id = c.IsoCode,
                name = c.Name,
            }).ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(array);
        }
        public string GetJsonSelectedCurrency()
        {
            var array = _currencies.Where(c => string.Compare(c.IsoCode, CurrencyISOCode, true) == 0).Select(c => new
            {
                id = c.IsoCode,
                name = c.Name,
            }).ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(array);
        }*/

        public IEnumerable<System.Web.Mvc.SelectListItem> GetCategories()
        {
            return _categories.Select(c => new System.Web.Mvc.SelectListItem
            {
                Value = c.Id,
                Text = c.Name,
                Selected = c.Id == Category
            }).ToList();
        }

        public IEnumerable<System.Web.Mvc.SelectListItem> GetCurrencies()
        {
            return _currencies.Select(c => new System.Web.Mvc.SelectListItem
            {
                Value = c.IsoCode,
                Text = c.Name,
                Selected = string.Compare(c.IsoCode, CurrencyISOCode, true) == 0
            }).ToList();
        }

        public IEnumerable<System.Web.Mvc.SelectListItem> GetDistributionKeys()
        {
            return _keys.Select(c => new System.Web.Mvc.SelectListItem
            {
                Value = c,
                Text = c,
                Selected = c == DistributionKey
            }).ToList();
        }

        public void When(LineCreated evnt)
        {
            LineId = evnt.LineId.ToString();
            BudgetId = evnt.BudgetId.ToString();
            Date = evnt.Expense.Date;
            Category = evnt.Expense.Category;
            Description = evnt.Expense.Description;
            Amount = evnt.Expense.Amount;
            CurrencyISOCode = evnt.Expense.Amount.GetCurrency().IsoCode;
        }

        public void When(LineExpenseChanged evnt)
        {
            Date = evnt.Expense.Date;
            Category = evnt.Expense.Category;
            Description = evnt.Expense.Description;
            Amount = evnt.Expense.Amount;
            CurrencyISOCode = evnt.Expense.Amount.GetCurrency().IsoCode;
        }

    }

}