using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Lines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class BudgetLinesProjection : InMemoryProjection
    {
        string _budget;
        List<Line> _lines = new List<Line>();


        public BudgetLinesProjection(string budget, IEventStoreConnection connection, UserCredentials credentials)
            : base(connection, credentials)
        {
            _budget = budget;
        }

        public BudgetLinesProjection(string budget, IPEndPoint endpoint, UserCredentials credentials)
            : base(endpoint, credentials)
        {
            _budget = budget;
        }


        protected override void Dispatch(dynamic evnt)
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
