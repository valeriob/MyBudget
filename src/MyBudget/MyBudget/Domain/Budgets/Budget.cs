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
    public class BudgetState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        BudgetId _id;
        string _name;
        UserId _owner;

        public void Apply(BudgetCreated evnt)
        {
            _id = evnt.BudgetId;
            _name = evnt.Name;
            _owner = evnt.Owner;
        }

        public bool CanRead(UserId userId)
        {
            return _owner.Equals(userId);
        }

    }

    public class BudgetCreated : Event
    {
        public BudgetId BudgetId { get; private set; }
        public string Name { get; private set; }
        public UserId Owner { get; private set; }

        public BudgetCreated(Guid id, DateTime timestamp, BudgetId budgetId, string name, UserId owner)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            Name = name;
            Owner = owner;
        }

    }

    public class BudgetId
    {
        string _id;

        public BudgetId(string _id)
        {
            this._id = _id;
        }


        public static BudgetId Create()
        {
            var id = "Budget-" + Guid.NewGuid();
            return new BudgetId(id);
        }
        public override string ToString()
        {
            return _id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BudgetId;
            return other != null && other._id == _id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }

    public class Budget : AggregateBase
    {
        BudgetState _state;

        public Budget(BudgetState state)
        {
            _state = state;
            Register<BudgetCreated>(e => Id = e.BudgetId.ToString());
        }


        public Budget() : this(new BudgetState())
        {

        }

        public void Create(BudgetId id, string name, UserId owner)
        {
            RaiseEvent(new BudgetCreated(Guid.NewGuid(), DateTime.Now, id, name, owner));
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
