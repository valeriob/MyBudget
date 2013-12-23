using CommonDomain.Persistence;
using MyBudget.Budgets;
using MyBudget.Budgets.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Commands
{
    public class AddUser : Command
    {
        public string UserId { get; set; }
        public UserLoginInfo UserLoginInfo { get; set; }
    }

    class UserHandlers : Handle<AddUser>
    {
        IRepository _repository;

        public UserHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(AddUser cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.Create(new UserId(cmd.UserId), cmd.UserLoginInfo);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }
    }
}
