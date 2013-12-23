using CommonDomain;
using CommonDomain.Core;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.Lines
{
    public class LineState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        LineCreated _creation;
        Amount _amount;

        public void Apply(LineCreated evnt)
        {
            _creation = evnt;
            _amount = evnt.Amount;
        }

        public void Apply(LineExpenseChanged evnt)
        {
            _amount = evnt.Amount;
        }

        public LineId GetLineId()
        {
            return _creation.LineId;
        }

        public BudgetId GetBudgetId()
        {
            return _creation.BudgetId;
        }
    }

    public class LineCreated : Event
    {
        public LineId LineId { get; private set; }
        public BudgetId BudgetId { get; private set; }
        public Amount Amount { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        public LineCreated(LineId lineId, BudgetId budgetId, Amount amount, DateTime timespan, string category, string desc)
        {
            LineId = lineId;
            BudgetId = budgetId;
            Timestamp = Timestamp;
            Amount = amount;
            Category = category;
            Description = desc;
        }
    }

    public class LineExpenseChanged : Event
    {
        public LineId LineId { get; private set; }
        public BudgetId BudgetId { get; private set; }
        public Amount Amount { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        public LineExpenseChanged(LineId lineId, BudgetId budgetId, Amount amount, DateTime timespan, string category, string desc)
        {
            LineId = lineId;
            BudgetId = budgetId;
            Timestamp = Timestamp;
            Amount = amount;
            Category = category;
            Description = desc;
        }
    }

    public class LineMarkedObsolete : Event
    {
        public LineId LineId { get; private set; }
        public BudgetId BudgetId { get; private set; }

        public LineMarkedObsolete(LineId lineId, BudgetId budget)
        {
            LineId = lineId;
            BudgetId = budget;
        }
    }



    public class LineId
    {
        string _id;
        public LineId(string id)
        {
            _id = id;
        }
        public LineId()
        {
            _id = "Line-" + Guid.NewGuid();
        }
    }

    public class Line : AggregateBase
    {
        LineState _state;
        public Line(LineState state)
        {
            _state = state;
        }

        public Line() : this(new LineState())
        {
        }

        public void Create(LineId id, BudgetId budgetId, Expense expense)
        {
            RaiseEvent(new LineCreated(id, budgetId, expense.Amount, expense.Timestamp, expense.Category, expense.Description));
        }

        public void Update(Expense expense)
        {
            RaiseEvent(new LineExpenseChanged(_state.GetLineId(), _state.GetBudgetId(), expense.Amount, expense.Timestamp, expense.Category, expense.Description));
        }

        public void MarkObsolete()
        {
            RaiseEvent(new LineMarkedObsolete(_state.GetLineId(), _state.GetBudgetId()));
        }


        protected override IMemento GetSnapshot()
        {
            return _state;
        }
    }

}
