using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Budgets;
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
    public class BudgetsListProjection : InMemoryProjection
    {
        Dictionary<string, Budget> _budgets = new Dictionary<string, Budget>();


        public BudgetsListProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
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

        public void When(BudgetCreated evnt)
        {
            if (_budgets.ContainsKey(evnt.BudgetId.ToString()))
                return;
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
            return userId.ToString() == Owner;
        }
    }

}
