using CommonDomain;
using CommonDomain.Core;
using MyBudget.Budgets.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets
{
    public class UserState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        UserLoginInfo _loginInfo;

        public void Apply(UserCreated evnt)
        {
            Id = evnt.UserId.ToString();
            _loginInfo = evnt.LoginInfo;
        }

        public UserLoginInfo GetUserLoginInfo()
        {
            return _loginInfo;
        }
    }

    public class UserId
    {
        string _id;
        public UserId(string id)
        {
            _id = id;
        }
        public UserId()
        {
            _id = "User_" + Guid.NewGuid();
        }
        public override string ToString()
        {
            return _id;
        }
    }

    public class UserCreated :  Event
    {
        public UserId UserId { get; set; }

        public UserLoginInfo LoginInfo { get; set; }
    }

    public class User : AggregateBase
    {
        UserState _state;
        public User(UserState state)
        {
            _state = state;
            Register<UserCreated>(e => Id = e.UserId.ToString());
        }

        public User(): this(new UserState()) { }


        public void Create(UserId userId, UserLoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("User already exists");

            RaiseEvent(new UserCreated { UserId = userId, LoginInfo = loginInfo });
        }

        protected override IMemento GetSnapshot()
        {
            return _state;
        }

    }

}
