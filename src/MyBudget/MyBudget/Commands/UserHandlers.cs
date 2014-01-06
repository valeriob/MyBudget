using CommonDomain.Persistence;
using MyBudget.Domain.Users;
using MyBudget.Domain.ValueObjects;
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
        [Obsolete("deprecato")]
        public UserLoginInfo UserLoginInfo { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AddUserLogin : Command
    {
        public string UserId { get; set; }
        public UserLoginInfo UserLoginInfo { get; set; }
    }
    public class RemoveUserLogin : Command
    {
        public string UserId { get; set; }
        public UserLoginInfo UserLoginInfo { get; set; }
    }
    public class AddUserPassword : Command
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
    public class ChangeUserPassword : Command
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    class UserHandlers : Handle<AddUser>, Handle<AddUserLogin>, Handle<RemoveUserLogin>, Handle<AddUserPassword>, Handle<ChangeUserPassword>
    {
        IRepository _repository;

        public UserHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(AddUser cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.Create(new UserId(cmd.UserId), cmd.UserLoginInfo, cmd.UserName, cmd.Password);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

        public void Handle(AddUserLogin cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.AddLogin(cmd.UserLoginInfo);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

        public void Handle(RemoveUserLogin cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.RemoveLogin(cmd.UserLoginInfo);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

        public void Handle(AddUserPassword cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.AddPassword(cmd.Password);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

        public void Handle(ChangeUserPassword cmd)
        {
            var user = _repository.GetById<User>(cmd.UserId);
            user.ChangePassword(cmd.OldPassword, cmd.NewPassword);
            _repository.Save(user, Guid.NewGuid(), cmd);
        }

    }

}
