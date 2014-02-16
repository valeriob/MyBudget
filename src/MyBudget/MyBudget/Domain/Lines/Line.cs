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
        public string CategoryId { get; private set; }
        public string Description { get; private set; }
        public UserId CreatedBy { get; private set; }
        public string[] Tags { get; private set; }

        public LineCreated(Guid id, DateTime timestamp, LineId lineId, BudgetId budgetId, Amount amount, DateTime date, string categoryId, string description, 
            string[] tags, UserId createdBy)
        {
            Id = id;
            Timestamp = timestamp;

            LineId = lineId;
            BudgetId = budgetId;
            Date = date;
            Amount = amount;
            CategoryId = categoryId;
            Description = description;
            CreatedBy = createdBy;
            Tags = tags;
        }
    }

    public class LineExpenseChanged : Event
    {
        public LineId LineId { get; private set; }
        public BudgetId BudgetId { get; private set; }
        public Amount Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string CategoryId { get; private set; }
        public string Description { get; private set; }
        public UserId UpdatedBy { get; private set; }


        public LineExpenseChanged(Guid id, DateTime timestamp, LineId lineId, BudgetId budgetId, Amount amount, DateTime date, string categoryId, string description, UserId updatedBy)
        {
            Id = id;
            Date = timestamp;
            LineId = lineId;
            BudgetId = budgetId;
            Date = date;
            Amount = amount;
            CategoryId = categoryId;
            Description = description;
            UpdatedBy = updatedBy;
        }
    }

    public class LineMarkedObsolete : Event
    {
        public LineId LineId { get; private set; }
        public BudgetId BudgetId { get; private set; }
        public UserId UserId { get; private set; }
        public LineId ObsoletedFor { get; set; }

        public LineMarkedObsolete(Guid id, DateTime timestamp, LineId lineId, BudgetId budget, UserId userId, LineId obsoletedFor)
        {
            Id = id;
            Timestamp = timestamp;
            LineId = lineId;
            BudgetId = budget;
            UserId = userId;
            ObsoletedFor = obsoletedFor;
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
            Register<LineCreated>(e => { Id = e.LineId.ToString(); _state.Apply(e); });
        }

        public Line()
            : this(new LineState())
        {
        }

        public void Create(LineId id, BudgetId budgetId, Expense expense, UserId createdBy, string[] tags)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("line already exists");

            RaiseEvent(new LineCreated(Guid.NewGuid(), DateTime.Now, id, budgetId, expense.Amount, expense.Timestamp, expense.Category, expense.Description, tags, createdBy));
        }

        public void Update(Expense expense, UserId updatedBy)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("line does not exists");

            RaiseEvent(new LineExpenseChanged(Guid.NewGuid(), DateTime.Now, _state.GetLineId(), _state.GetBudgetId(), expense.Amount, expense.Timestamp, expense.Category, expense.Description, updatedBy));
        }

        public void MarkObsolete(UserId userId, LineId obsoletedFor)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("line does not exists");

            RaiseEvent(new LineMarkedObsolete(Guid.NewGuid(), DateTime.Now, _state.GetLineId(), _state.GetBudgetId(), userId, obsoletedFor));
        }


        protected override IMemento GetSnapshot()
        {
            return _state;
        }
    }

}
