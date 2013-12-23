using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class BudgetsListProjection : InMemoryProjection
    {
        Dictionary<string, Budget> _budgets = new Dictionary<string, Budget>();


        public BudgetsListProjection(IEventStoreConnection connection, UserCredentials credentials)
            : base(connection, credentials)
        {
        }


        public override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

        public void When(BudgetCreated evnt)
        {
            var s = new Budget();
            s.Apply(evnt);
            _budgets.Add(evnt.BudgetId.ToString(), s);
        }

        public IEnumerable<Budget> GetBudgetsUserCanView(UserId userId)
        {
            return _budgets.Values.Where(b => b.CanRead(userId));
        }

        public Budget GetBudgetById(BudgetId budgetId)
        {
            return _budgets[budgetId.ToString()];
        }

    }

    public class Budget
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Owner { get; set; }

        public void Apply(BudgetCreated evnt)
        {
            Id = evnt.BudgetId.ToString();
            Name = evnt.Name;
            Owner = evnt.Owner.ToString();
            Created = evnt.Timestamp;
        }

        internal bool CanRead(UserId userId)
        {
            return userId.ToString() == Id;
        }
    }

}
