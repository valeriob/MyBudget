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

    public class CreateCategory : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
    }
    public class UpdateCategory : Command
    {
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
    }


    class BudgetHandlers : Handle<CreateBudget>, Handle<AllowBudgetAccess>, Handle<CreateCategory>, Handle<UpdateCategory>
    {
        IRepository _repository;

        public BudgetHandlers(IRepository repository)
        {
            _repository = repository;
        }


        public void Handle(CreateBudget cmd)
        {
            var budget = _repository.GetById<Budget>(cmd.BudgetId);
            budget.Create(new BudgetId(cmd.BudgetId), cmd.BudgetName, new UserId(cmd.UserId));
            _repository.Save(budget, Guid.NewGuid(), cmd);
        }

        public void Handle(AllowBudgetAccess cmd)
        {
            var budget = _repository.GetById<Budget>(cmd.BudgetId);
            budget.AllowAccess(new UserId(cmd.UserId), new UserId(cmd.UserIdAllowed));
            _repository.Save(budget, Guid.NewGuid(), cmd);
        }

        public void Handle(CreateCategory cmd)
        {
            var category = _repository.GetById<Category>(cmd.CategoryId);
            category.Create(cmd.BudgetId, cmd.CategoryId, cmd.CategoryName, cmd.CategoryDescription);
            _repository.Save(category, Guid.NewGuid(), cmd);
        }


        public void Handle(UpdateCategory cmd)
        {
            var category = _repository.GetById<Category>(cmd.CategoryId);
            category.Update(cmd.CategoryName, cmd.CategoryDescription);
            _repository.Save(category, Guid.NewGuid(), cmd);
        }
    }

}
