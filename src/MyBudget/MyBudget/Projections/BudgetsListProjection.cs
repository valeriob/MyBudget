using EventStore.ClientAPI;
using MyBudget.Budgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class BudgetsListProjection : InMemoryProjection
    {
        IEventStoreConnection _connection;
        List<Budget> _budgets = new List<Budget>();

        public BudgetsListProjection(IEventStoreConnection connection)
        {
            _connection = connection;
        }

        public override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch { }
        }
        public void When(BudgetCreated evnt)
        {
            _budgets.Add(new Budget());
        }
    }
}
