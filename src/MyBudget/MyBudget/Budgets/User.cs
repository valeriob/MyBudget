using CommonDomain;
using CommonDomain.Core;
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
    }

    public class UserCreated :  Event
    {

    }

    public class User : AggregateBase
    {
        UserState _state;
        public User(UserState state)
        {
            _state = state;
        }

        public User()
            : this(new UserState())
        {

        }

        public void Create(UserId id)
        {

        }

        public void Disable()
        {

        }
        public void Enable()
        {

        }
    }

}
