﻿using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
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

    public class BudgetLinesPagedViewModel
    {
        public IEnumerable<BudgetLine> Lines { get; private set; }
        public string BudgetId { get; private set; }
        public DateTime? From { get; private set; }
        public DateTime? To { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public BudgetLinesPagedViewModel(string budgetId, PagedResult<BudgetLine> lines, DateTime? from, DateTime? to)
        {
            From = from;
            To = to;
            BudgetId = budgetId;
            Lines = lines;
            PageIndex = lines.PageIndex;
            TotalPages = lines.TotalPages();
        }

        public bool PrevLinkVisible()
        {
            return PageIndex > 0;
        }
        public bool NextLinkVisible()
        {
            return PageIndex < TotalPages;
        }
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
        public string BudgetName { get; set; }

        IEnumerable<string> _categories;
        IEnumerable<Currency> _currencies;


        [Obsolete("Serialization", true)]
        public EditBudgetLineViewModel()
        {

        }

        public EditBudgetLineViewModel(string budgetName, string budgetId, IEnumerable<string> categories, IEnumerable<Currency> currencies)
        {
            BudgetName = budgetName;
            _categories = categories.ToList();
            _currencies = currencies;

            BudgetId = budgetId;
            LineId = MyBudget.Domain.Lines.LineId.Create(new MyBudget.Domain.Budgets.BudgetId(budgetId)).ToString();
            Date = DateTime.Now;
            CurrencyISOCode = Currencies.Euro().IsoCode;
        }

        public EditBudgetLineViewModel(string budgetName, IEnumerable<dynamic> events, IEnumerable<string> categories, IEnumerable<Currency> currencies)
            : this(budgetName, "", categories, currencies)
        {
            EventHelper.Apply(events, this);
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
                Value = c,
                Text = c,
                Selected = string.Compare(c, Category, true) == 0
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