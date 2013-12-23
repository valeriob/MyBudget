using CommonDomain;
using CommonDomain.Core;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.Users
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

        public bool Is(string userId)
        {
            return Id == userId;
        }
    }

    public class UserId
    {
        string _id;

        public UserId(string _id)
        {
            this._id = _id;
        }

        public override string ToString()
        {
            return _id;
        }
        public override bool Equals(object obj)
        {
            var other = obj as UserId;
            return other != null && other._id == _id;
        }

        public static UserId CreateNew()
        {
            var id = "User-" + Guid.NewGuid();
            return new UserId(id);
        }
    }

    public class UserCreated : Event
    {
        public UserId UserId { get; set; }
        public UserLoginInfo LoginInfo { get; set; }


        public UserCreated(Guid id, DateTime timestamp, UserId userId, UserLoginInfo loginInfo)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            LoginInfo = loginInfo;
        }

        //UserCreated(Guid id, DateTime timestamp, UserId userId, UserLoginInfo loginInfo) : base(id, timestamp)
        //{
        //    UserId = userId;
        //    LoginInfo = loginInfo;
        //}
    }

    public class User : AggregateBase
    {
        UserState _state;
        public User(UserState state)
        {
            _state = state;
            Register<UserCreated>(e => Id = e.UserId.ToString());
        }

        public User() : this(new UserState()) { }


        public void Create(UserId userId, UserLoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("User already exists");

            RaiseEvent(new UserCreated(Guid.NewGuid(), DateTime.Now, userId, loginInfo));
        }

        protected override IMemento GetSnapshot()
        {
            return _state;
        }

    }

}
