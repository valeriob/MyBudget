using CommonDomain.Persistence;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Commands
{
    public class CreateBudget : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
    }

    public class AllowBudgetAccess : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string UserIdAllowed { get; set; }
    }

    class BudgetHandlers : Handle<CreateBudget>, Handle<AllowBudgetAccess>
    {
        IRepository _repository;

        public BudgetHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(CreateBudget cmd)
        {
            var user = _repository.GetById<Budget>(cmd.BudgetId);
            user.Create(new BudgetId(cmd.BudgetId), cmd.BudgetName, new UserId(cmd.UserId));
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

        public void Handle(AllowBudgetAccess cmd)
        {
            var user = _repository.GetById<Budget>(cmd.BudgetId);
            user.AllowAccess(new UserId(cmd.UserId), new UserId(cmd.UserIdAllowed));
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

    }

}
