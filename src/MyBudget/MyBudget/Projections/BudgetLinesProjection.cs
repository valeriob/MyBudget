using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
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


        public BudgetLinesProjection(string budget, IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter)
            : base(endpoint, credentials, adapter, null)
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
            if (evnt.BudgetId.ToString().Equals(_budget) == false)
                return;

            if (_lines.Any(l => l.Id == evnt.LineId.ToString()))
                return;
            _lines.Add(new BudgetLine(evnt));
        }

        void When(LineExpenseChanged evnt)
        {
            if (evnt.BudgetId.ToString().Equals(_budget) == false)
                return;

            var line = _lines.Single(l => l.Id == evnt.LineId.ToString());
            line.When(evnt);
        }

        public IEnumerable<BudgetLine> GetAllLines()
        {
            return _lines;
        }

        public BudgetLine GetLine(string id)
        {
            return _lines.Single(l => l.Id == id);
        }

        //public IEnumerable<dynamic> GetLineEvents(string lineId)
        //{
        //    var slice = GetConnection().ReadStreamEventsForward(lineId, 0, int.MaxValue, true, GetUserCredentials());
        //    return GetEventsFrom(slice);
        //}
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

        internal void When(LineExpenseChanged evnt)
        {
            Amount = evnt.Amount;
            Category = evnt.Category;
            Description = evnt.Description;
            Date = evnt.Date;
        }
    }
}
