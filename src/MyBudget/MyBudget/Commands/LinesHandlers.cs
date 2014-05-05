using CommonDomain.Persistence;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.Users;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Commands
{
    public class CreateLine : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string LineId { get; set; }

        public Expense Expense { get; set; }
        //public Amount Amount { get; set; }
        //public DateTime Date { get; set; }
        //public string CategoryId { get; set; }
        //public string DistributionKey { get; set; }
        //public string Description { get; set; }
        //public string[] Tags { get; set; }
    }

    public class MarkLineAsObsolete : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string LineId { get; set; }

        public string ObsoletedForLineId { get; set; }
    }

    public class UpdateLine : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string LineId { get; set; }

        public Expense Expense { get; set; }
    }

    class LinesHandlers : Handle<CreateLine>, Handle<MarkLineAsObsolete>, Handle<UpdateLine>
    {
        IRepository _repository;

        public LinesHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(CreateLine cmd)
        {
            var budget = _repository.GetById<Budget>(cmd.BudgetId);
            budget.EnsureCurrencyIsCorrect(cmd.Expense.Amount.GetCurrency().IsoCode);

            var line = _repository.GetById<Line>(cmd.LineId);
            line.Create(new LineId(cmd.LineId), new BudgetId(cmd.BudgetId), cmd.Expense, new UserId(cmd.UserId));
            _repository.Save(line, Guid.NewGuid(), cmd);
        }

        public void Handle(MarkLineAsObsolete cmd)
        {
            var line = _repository.GetById<Line>(cmd.LineId);
            line.MarkObsolete(new UserId(cmd.UserId), new LineId(cmd.ObsoletedForLineId));
            _repository.Save(line, Guid.NewGuid(), cmd);
        }


        public void Handle(UpdateLine cmd)
        {
            var line = _repository.GetById<Line>(cmd.LineId);
            line.Update(cmd.Expense, new UserId(cmd.UserId));
            _repository.Save(line, Guid.NewGuid(), cmd);
        }
    }
}
