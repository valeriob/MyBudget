using CommonDomain;
using CommonDomain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets
{
    public class BudgetState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        BudgetId _id;
        string _name;
        AccountId _owner;
        public void Apply(BudgetCreated evnt)
        {
            _id = evnt.BudgetId;
            _name = evnt.Name;
            _owner = evnt.Owner;
        }
    }

    public class BudgetCreated : Event
    {
        public BudgetCreated(BudgetId id, string name, AccountId owner)
        {
            BudgetId = id;
            Name = name;
            Owner = owner;
        }

        public BudgetId BudgetId { get; private set; }
        public string Name { get; private set; }
        public AccountId Owner { get; private set; }
    }

    public class BudgetId
    {
        string _id;
        public BudgetId(string id)
        {
            _id = id;
        }
        public BudgetId()
        {
            _id = "Budget_" + Guid.NewGuid();
        }
    }

    public class Budget : AggregateBase
    {
        BudgetState _state;
        public Budget(BudgetState state)
        {
            _state = state;
        }

        public Budget() : this(new BudgetState())
        {

        }

        public void Create(BudgetId id, string name, AccountId owner)
        {
            RaiseEvent(new BudgetCreated(id, name, owner));
        }

        public void AllowReadAccess(AccountId accountId)
        {

        }
        public void AllowWriteAccess(AccountId accountId)
        {

        }
        protected override IMemento GetSnapshot()
        {
            return _state;
        }
    }

}
