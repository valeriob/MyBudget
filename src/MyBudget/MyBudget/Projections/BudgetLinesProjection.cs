using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
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
        List<BudgetLine> _lines = new List<BudgetLine>();
        public string BudgetId { get { return _budget; } }


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
            try
            {
                ((dynamic)this).When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

        void When(LineCreated evnt)
        {
            _lines.Add(new BudgetLine(evnt));
        }

        public IEnumerable<BudgetLine> GetAllLines()
        {
            return _lines;
        }
    }

    public class BudgetLine
    {
        public string Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public DateTime Date { get; private set; }
        public Amount Amount { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        public BudgetLine(LineCreated evnt)
        {
            Id = evnt.LineId.ToString();
            Timestamp = evnt.Timestamp;
            Date = evnt.Date;
            Amount = evnt.Amount;
            Category = evnt.Category;
            Description = evnt.Description;
        }

    }
}
