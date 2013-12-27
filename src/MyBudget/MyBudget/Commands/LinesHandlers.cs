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

        public Amount Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    class LinesHandlers : Handle<CreateLine>
    {
        IRepository _repository;

        public LinesHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(CreateLine cmd)
        {
            var expense = new MyBudget.Domain.ValueObjects.Expense(cmd.Amount, cmd.Date, cmd.Category, cmd.Description);

            var user = _repository.GetById<Line>(cmd.UserId);
            user.Create(new LineId(cmd.LineId), new BudgetId(cmd.BudgetId), expense, new UserId(cmd.UserId));
            _repository.Save(user, Guid.NewGuid(), cmd);
        }
    }
}
