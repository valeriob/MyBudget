using CommonDomain.Persistence;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Users;
using MyBudget.Infrastructure;
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

        public string CurrencyISOCode { get; set; }
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

    public class AddBudgetDistributionKey : Command
    {
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public string Name { get; set; }
    }

    public class SubmitDistributionCheckPoint : Command
    {
        public string CheckPointId { get; set; }
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public DateTime Date { get; set; }
        public DistributionKeyAmount[] Amounts { get; set; }
    }


    class BudgetHandlers : Handle<CreateBudget>, Handle<AllowBudgetAccess>, Handle<CreateCategory>,
        Handle<UpdateCategory>, Handle<AddBudgetDistributionKey>, Handle<SubmitDistributionCheckPoint>
    {
        IRepository _repository;
        IEventStore _eventStore;

        public BudgetHandlers(IRepository repository, IEventStore eventStore)
        {
            _repository = repository;
            _eventStore = eventStore;
        }


        public void Handle(CreateBudget cmd)
        {
            var budget = _repository.GetById<Budget>(cmd.BudgetId);
            budget.Create(new BudgetId(cmd.BudgetId), cmd.BudgetName, new UserId(cmd.UserId), cmd.CurrencyISOCode);
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


        public void Handle(AddBudgetDistributionKey cmd)
        {
            var budget = _repository.GetById<Budget>(cmd.BudgetId);
            budget.AddDistributionKey(cmd.Name);
            _repository.Save(budget, Guid.NewGuid(), cmd);
        }

        public void Handle(SubmitDistributionCheckPoint cmd)
        {
            var evnt = new CheckPointsubmitted
            {
                CheckPointId = cmd.CheckPointId,
                UserId = cmd.UserId,
                BudgetId = cmd.BudgetId,
                Amounts = cmd.Amounts,
                Date = cmd.Date
            };

            _eventStore.Save(cmd.CheckPointId, evnt);
        }
    }



}
