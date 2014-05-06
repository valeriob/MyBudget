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
    public class DistributionKeyState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        string _id;
        string _name;
        string _description;
        string _budgetId;

        public void Apply(DistributionKeyCreated evnt)
        {
            _id = evnt.DistributionKeyId;
            _name = evnt.Name;
            _description = evnt.Description;
            _budgetId = evnt.BudgetId;
        }
        public void Apply(DistributionKeyUpdated evnt)
        {
            _name = evnt.Name;
            _description = evnt.Description;
        }

        public string BudgetId()
        {
            return _budgetId;
        }
    }

    public class DistributionKeyCreated : Event
    {
        public string BudgetId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string DistributionKeyId { get; set; }


        public DistributionKeyCreated(Guid id, DateTime timestamp, string budgetId, string distributionKeyId, string name, string description)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            DistributionKeyId = distributionKeyId;
            Name = name;
            Description = description;
        }
    }
    public class DistributionKeyUpdated : Event
    {
        public string BudgetId { get; private set; }
        public string DistributionKeyId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public DistributionKeyUpdated(Guid id, DateTime timestamp, string budgetId, string distributionKeyId, string name, string description)
        {
            Id = id;
            Timestamp = timestamp;
            BudgetId = budgetId;
            DistributionKeyId = distributionKeyId;
            Name = name;
            Description = description;
        }
    }


    public class DistributionKey : AggregateBase
    {
        DistributionKeyState _state;

        public DistributionKey(DistributionKeyState state)
        {
            _state = state;
            Register<DistributionKeyCreated>(e => { Id = e.DistributionKeyId; _state.Apply(e); });
        }


        public DistributionKey()
            : this(new DistributionKeyState())
        {

        }

        public void Create(string budgetId, string DistributionKeyId, string name, string description)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("exists");

            RaiseEvent(new DistributionKeyCreated(Guid.NewGuid(), DateTime.Now, budgetId, DistributionKeyId, name, description));
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("does not exists");
            RaiseEvent(new DistributionKeyUpdated(Guid.NewGuid(), DateTime.Now, _state.BudgetId(), Id, name, description));
        }

        protected override IMemento GetSnapshot()
        {
            return _state;
        }



    }

}
