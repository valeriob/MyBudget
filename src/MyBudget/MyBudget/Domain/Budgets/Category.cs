using CommonDomain;
using CommonDomain.Core;
using MyBudget.Domain.Accounts;
using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.Budgets
{
    public class CategoryState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        string _id;
        string _name;
        string _description;
        string _budgetId;

        public void Apply(CategoryCreated evnt)
        {
            _id = evnt.CategoryId;
            _name = evnt.Name;
            _description = evnt.Description;
            _budgetId = evnt.BudgetId;
        }
        public void Apply(CategoryUpdated evnt)
        {
            _name = evnt.Name;
            _description = evnt.Description;
        }

        public string BudgetId()
        {
            return _budgetId;
        }
    }

    public class CategoryCreated : Event
    {
        public string BudgetId { get; private set; }
        public string CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public CategoryCreated(Guid id, DateTime timestamp, string budgetId, string categoryId, string name, string description)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            CategoryId = categoryId;
            Name = name;
            Description = description;
        }
    }
    public class CategoryUpdated : Event
    {
        public string BudgetId { get; private set; }
        public string CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public CategoryUpdated(Guid id, DateTime timestamp, string budgetId, string categoryId, string name, string description)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            CategoryId = categoryId;
            Name = name;
            Description = description;
        }
    }


    public class Category : AggregateBase
    {
        CategoryState _state;

        public Category(CategoryState state)
        {
            _state = state;
            Register<CategoryCreated>(e => { Id = e.CategoryId; _state.Apply(e); });
        }


        public Category()
            : this(new CategoryState())
        {

        }

        public void Create(string budgetId, string categoryId, string name, string description)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("exists");

            RaiseEvent(new CategoryCreated(Guid.NewGuid(), DateTime.Now, budgetId, categoryId, name, description));
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("does not exists");
            RaiseEvent(new CategoryUpdated(Guid.NewGuid(), DateTime.Now, _state.BudgetId(), Id, name, description));
        }

        protected override IMemento GetSnapshot()
        {
            return _state;
        }



    }

}
