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

        public void Apply(BudgetAccessAllowed evnt)
        {

        }
        

        public bool CanAllowAccess(UserId userId)
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

    public class BudgetAccessAllowed : Event
    {
        public BudgetId BudgetId { get; private set; }
        public UserId AllowedUserId { get; private set; }

        public BudgetAccessAllowed(Guid id, DateTime timestamp, BudgetId budgetId, UserId allowedUserId)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            AllowedUserId = allowedUserId;
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
            var id = "Budgets-" + Guid.NewGuid().ToString().Replace('-','_');
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
            Register<BudgetCreated>(e => { Id = e.BudgetId.ToString(); _state.Apply(e); });
        }


        public Budget() : this(new BudgetState())
        {

        }

        public void Create(BudgetId id, string name, UserId owner)
        {
            RaiseEvent(new BudgetCreated(Guid.NewGuid(), DateTime.Now, id, name, owner));
        }

        public void AllowAccess(UserId fromUser, UserId allowedUserId)
        {
            if (_state.CanAllowAccess(fromUser) == false)
                throw new Exception("User cannot allow access to budget");

            RaiseEvent(new BudgetAccessAllowed(Guid.NewGuid(), DateTime.Now, new BudgetId(Id), allowedUserId));
        }
        //public void AllowWriteAccess(AccountId accountId)
        //{

        //}

        protected override IMemento GetSnapshot()
        {
            return _state;
        }

    }

}
