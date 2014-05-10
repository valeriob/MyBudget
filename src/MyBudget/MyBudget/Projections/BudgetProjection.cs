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
    public interface IBudgetProjection
    {
        string Name { get; }
        string CurrencyISOCode { get; }

        IEnumerable<BudgetLine> GetAllLines();
        BudgetLine GetLine(string id);
        IEnumerable<BudgetLine> GetAllLinesBetween(DateTime? from, DateTime? to);
        PagedResult<BudgetLine> GetAllLinesPaged(int page, DateTime? from, DateTime? to, string category);

        IEnumerable<CheckPoint> GetCheckPoints();
    }


    public class BudgetProjection : InMemoryProjection, IBudgetProjection
    {
        readonly static int _pageSize = 15;

        Budget _budget;
        List<BudgetLine> _lines;
        Dictionary<string, CheckPoint> _checkPoints;

        public string Name { get { return _budget.Name; } }
        public string CurrencyISOCode { get { return _budget.CurrencyISOCode; } }


        public BudgetProjection(Budget budget, IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string stream)
            : base(endpoint, credentials, adapter, stream)
        {
            _budget = budget;

            _checkPoints = new Dictionary<string, CheckPoint>();
            _lines = new List<BudgetLine>();
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
            if (evnt.BudgetId.ToString().Equals(_budget.Id) == false)
                return;

            if (_lines.Any(l => l.Id == evnt.LineId.ToString()))
                return;
            _lines.Add(new BudgetLine(evnt));
        }

        void When(LineExpenseChanged evnt)
        {
            if (evnt.BudgetId.ToString().Equals(_budget.Id) == false)
                return;

            var line = _lines.Single(l => l.Id == evnt.LineId.ToString());
            line.When(evnt);
        }

        void When(MyBudget.Domain.Budgets.CheckPointsubmitted evnt)
        {
            //CheckPoint value;
            //if(_checkPoints.TryGetValue(evnt.CheckPointId, out value) == false)
            //{
            //    value = new CheckPoint { Id = evnt.CheckPointId };
            //}

            _checkPoints[evnt.CheckPointId] = new CheckPoint
            {
                Id = evnt.CheckPointId,
                UserId = evnt.UserId,
                Date = evnt.Date,
                BudgetId = evnt.BudgetId,
                Amounts = evnt.Amounts
            };
        }


        public IEnumerable<BudgetLine> GetAllLines()
        {
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

        public IEnumerable<CheckPoint> GetCheckPoints()
        {
            return _checkPoints.Values;
        }

    }

    public class CheckPoint
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public DateTime Date { get; set; }
        public MyBudget.Domain.Budgets.DistributionKeyAmount[] Amounts { get; set; }
    }

}
