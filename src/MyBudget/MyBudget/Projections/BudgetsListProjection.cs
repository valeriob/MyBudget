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
        Dictionary<string, BudgetState> _budgets = new Dictionary<string, BudgetState>();


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
            var s = new BudgetState();
            s.Apply(evnt);
            _budgets.Add(evnt.BudgetId.ToString(), s);
        }

        public IEnumerable<BudgetState> GetBudgetsUserCanView(UserId userId)
        {
            return _budgets.Values.Where(b => b.CanRead(userId));
        }

        public BudgetState GetBudgetById(BudgetId budgetId)
        {
            return _budgets[budgetId.ToString()];
        }

    }

}
