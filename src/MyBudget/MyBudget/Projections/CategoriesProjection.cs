﻿using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.Users;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public interface ICategoriesProjection
    {
        IEnumerable<Category> GetBudgetsCategories(BudgetId budgetId);
        IEnumerable<Category> GetBudgetsCategories(string budgetId);
    }

    public class CategoriesProjection : InMemoryProjection, ICategoriesProjection
    {
        Dictionary<string, List<Category>> _categories = new Dictionary<string, List<Category>>();


        public CategoriesProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
            : base(endpoint, credentials, adapter, streamName)
        {
        }



        protected override void Dispatch(dynamic evnt)
        {
            try
            {
                ((dynamic)this).When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

        //void When(LineCreated evnt)
        //{
        //    AddCategory(evnt.BudgetId.ToString(), evnt.Category);
        //}

        //void When(LineExpenseChanged evnt)
        //{
        //    AddCategory(evnt.BudgetId.ToString(), evnt.Category);
        //}

        void When(CategoryCreated evnt)
        {
            AddCategory(evnt.BudgetId, evnt.CategoryId, evnt.Name, evnt.Description);
        }
        void When(CategoryUpdated evnt)
        {
            UpdateCategory(evnt.BudgetId, evnt.CategoryId, evnt.Name, evnt.Description);
        }

        void AddCategory(string budget, string id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            List<Category> categories;
            if (_categories.TryGetValue(budget, out categories) == false)
                _categories[budget] = categories = new List<Category>();

            if (categories.Any(c => c.Id == id) == false)
                categories.Add(new Category { Id = id, Name = name, Description = description });
        }
        void UpdateCategory(string budget, string id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            List<Category> categories;
            if (_categories.TryGetValue(budget, out categories) == false)
                _categories[budget] = categories = new List<Category>();

            var category = categories.Single(s => s.Id == id);
            category.Name = name;
            category.Description = description;
        }

        public IEnumerable<Category> GetBudgetsCategories(BudgetId budgetId)
        {
            List<Category> categories;
            if (_categories.TryGetValue(budgetId.ToString(), out categories) == false)
                _categories[budgetId.ToString()] = categories = new List<Category>();
            return categories.OrderBy(d => d.Name);
        }

        public IEnumerable<Category> GetBudgetsCategories(string budgetId)
        {
            List<Category> categories;
            if (_categories.TryGetValue(budgetId.ToString(), out categories) == false)
                _categories[budgetId] = categories = new List<Category>();
            return categories.OrderBy(d => d.Name);
        }

    }

    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
