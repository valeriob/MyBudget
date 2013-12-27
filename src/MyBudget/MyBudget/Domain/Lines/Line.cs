using CommonDomain;
using CommonDomain.Core;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Users;
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
        public DateTime Date { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }
        public UserId CreatedBy { get; private set; }

        public LineCreated(Guid id, DateTime timestamp, LineId lineId, BudgetId budgetId, Amount amount, DateTime date, string category, string description, UserId createdBy)
        {
            Id = id;
            Timestamp = timestamp;

            LineId = lineId;
            BudgetId = budgetId;
            Date = date;
            Amount = amount;
            Category = category;
            Description = description;
            CreatedBy = createdBy;
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

        public LineId(string _id)
        {
            this._id = _id;
        }


        public static LineId Create(BudgetId budgetId)
        {
            var id = "Lines-" + Guid.NewGuid().ToString().Replace('-', '_');
            return new LineId(id);
        }
        public override string ToString()
        {
            return _id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LineId;
            return other != null && other._id == _id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }

    public class Line : AggregateBase
    {
        LineState _state;
        public Line(LineState state)
        {
            _state = state;
            Register<LineCreated>(e => Id = e.LineId.ToString());
        }

        public Line() : this(new LineState())
        {
        }

        public void Create(LineId id, BudgetId budgetId, Expense expense, UserId createdBy)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("line already exists");

            RaiseEvent(new LineCreated(Guid.NewGuid(), DateTime.Now, id, budgetId, expense.Amount, expense.Timestamp, expense.Category, expense.Description, createdBy));
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
