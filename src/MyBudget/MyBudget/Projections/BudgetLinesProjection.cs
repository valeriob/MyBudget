using EventStore.ClientAPI;
using MyBudget.Budgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class BudgetLinesProjection : InMemoryProjection
    {
        string _budget;
        IEventStoreConnection _connection;
        List<Line> _lines = new List<Line>();


        public BudgetLinesProjection(string budget, IEventStoreConnection connection)
        {
            _budget = budget;
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
        public void When(LineCreated evnt)
        {
            _lines.Add(new Line());
        }
    }
}
