﻿using EventStore.ClientAPI;
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

        readonly static int _pageSize = 15;
        public PagedResult<BudgetLine> GetAllLinesPaged(int page, DateTime? from, DateTime? to)
        {
            var q = _lines.AsEnumerable();
            if (from.HasValue)
                q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue)
                q = q.Where(r => r.Date <= to.Value);
            q = q.OrderByDescending(d => d.Date);

            q = q.Skip(_pageSize * page).Take(_pageSize);

            return new PagedResult<BudgetLine>(q.ToList(), _lines.Count, page, _pageSize);
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
