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
        public string BudgetName { get; set; }

        IEnumerable<string> _categories;
        IEnumerable<Currency> _currencies;


        [Obsolete("Serialization", true)]
        public EditBudgetLineViewModel()
        {

        }

        public EditBudgetLineViewModel(string budgetName, IEnumerable<dynamic> events, IEnumerable<string> categories, IEnumerable<Currency> currencies)
        {
            EventHelper.Apply(events, this);
            _categories = categories.ToList();
            BudgetName = budgetName;
            _currencies = currencies;
        }

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
            var array = _categories.Where(c=> string.Compare(c, Category, true) == 0).Select(c => new
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
        }
        /*
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
        }*/

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