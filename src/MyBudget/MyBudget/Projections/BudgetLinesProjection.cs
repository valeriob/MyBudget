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
    public interface IBudgetLinesProjection
    {
        IEnumerable<BudgetLine> GetAllLines();
        Task<IEnumerable<BudgetLine>> GetAllLines(DateTime last);

        BudgetLine GetLine(string id);
        IEnumerable<BudgetLine> GetAllLinesBetween(DateTime? from, DateTime? to);
        PagedResult<BudgetLine> GetAllLinesPaged(int page, DateTime? from, DateTime? to, string category);

      
    }


    public class BudgetLinesProjection : InMemoryProjection, IBudgetLinesProjection
    {
        readonly static int _pageSize = 15;
        string _budget;
        List<BudgetLine> _lines = new List<BudgetLine>();
        public string BudgetId { get { return _budget; } }



        public BudgetLinesProjection(string budget, IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter)
            : base(endpoint, credentials, adapter)
        {
            _budget = budget;
        }

        public BudgetLinesProjection(string budget, IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string stream)
            : base(endpoint, credentials, adapter, stream)
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
            {
                return;
                System.Diagnostics.Debugger.Break();
            }
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
            return _lines.OrderBy(d => d.Date);
        }

        public async Task<IEnumerable<BudgetLine>> GetAllLines(DateTime until)
        {
            while (LastUpdate < until)
                await Task.Delay(100);

            return _lines.OrderBy(d => d.Date);
        }

        public BudgetLine GetLine(string id)
        {
            return _lines.Single(l => l.Id == id);
        }

        public IEnumerable<BudgetLine> GetAllLinesBetween(DateTime? from, DateTime? to)
        {
            var q = _lines.AsEnumerable();
            if (from.HasValue)
                q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue)
                q = q.Where(r => r.Date <= to.Value);
            return q.OrderBy(d => d.Date);
        }

        public PagedResult<BudgetLine> GetAllLinesPaged(int page, DateTime? from, DateTime? to, string category)
        {
            var q = _lines.AsEnumerable();
            if (from.HasValue)
                q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue)
                q = q.Where(r => r.Date <= to.Value);

            if (string.IsNullOrEmpty(category) == false)
                q = q.Where(r => r.Category == category);

            var total = q.Count();

            q = q.OrderByDescending(d => d.Date);

            q = q.Skip(_pageSize * page).Take(_pageSize);

            return new PagedResult<BudgetLine>(q.ToList(), total, page, _pageSize);
        }
    }

    public class PagedResult<T> : IEnumerable<T>
    {
        public IEnumerable<T> Items { get; private set; }
        public int TotalItems { get; private set; }
        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }

        public PagedResult(IEnumerable<T> items, int totalItems, int pageIndex, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int TotalPages()
        {
            return TotalItems / PageSize;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
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
        public string DistributionKey { get; set; }
        public string[] Tags { get; set; }

        public BudgetLine(LineCreated evnt)
        {
            Id = evnt.LineId.ToString();
            Timestamp = evnt.Timestamp;
            Date = evnt.Expense.Date;
            Amount = evnt.Expense.Amount;
            Category = evnt.Expense.Category;
            Description = evnt.Expense.Description;
            DistributionKey = evnt.Expense.DistributionKey;
            Tags = evnt.Expense.Tags;
        }

        internal void When(LineExpenseChanged evnt)
        {
            Amount = evnt.Expense.Amount;
            Category = evnt.Expense.Category;
            Description = evnt.Expense.Description;
            Date = evnt.Expense.Date;
            DistributionKey = evnt.Expense.DistributionKey;
            Tags = evnt.Expense.Tags;
        }
    }
}
