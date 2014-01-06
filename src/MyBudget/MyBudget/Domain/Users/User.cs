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
        public void Apply(UserPasswordChanged evnt)
        { }
        public void Apply(UserLoginAdded evnt)
        { }
        public void Apply(UserLoginRemoved evnt)
        { }

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
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public static UserId CreateNew()
        {
            var id = "Users-" + Guid.NewGuid().ToString().Replace('-', '_');
            return new UserId(id);
        }
    }

    public class UserCreated : Event
    {
        public UserId UserId { get; set; }
        public UserLoginInfo LoginInfo { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserCreated(Guid id, DateTime timestamp, UserId userId, UserLoginInfo loginInfo, string userName, string password)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            LoginInfo = loginInfo;
            UserName = userName;
            Password = password;
        }
    }

    public class UserLoginAdded : Event
    {
        public UserId UserId { get; set; }
        public UserLoginInfo LoginInfo { get; set; }

        public UserLoginAdded(Guid id, DateTime timestamp, UserId userId, UserLoginInfo loginInfo)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            LoginInfo = loginInfo;
        }
    }

    public class UserLoginRemoved : Event
    {
        public UserId UserId { get; set; }
        public UserLoginInfo LoginInfo { get; set; }

        public UserLoginRemoved(Guid id, DateTime timestamp, UserId userId, UserLoginInfo loginInfo)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            LoginInfo = loginInfo;
        }
    }

    public class UserPasswordChanged : Event
    {
        public UserId UserId { get; set; }
        public string Password { get; set; }

        public UserPasswordChanged(Guid id, DateTime timestamp, UserId userId, string password)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            Password = password;
        }
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


        public void Create(UserId userId, UserLoginInfo loginInfo, string userName, string password)
        {
            if (string.IsNullOrEmpty(Id) == false)
                throw new Exception("User already exists");

            RaiseEvent(new UserCreated(Guid.NewGuid(), DateTime.Now, userId, loginInfo, userName, password));
        }

        internal void AddLogin(UserLoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("User does not exists");

            RaiseEvent(new UserLoginAdded(Guid.NewGuid(), DateTime.Now, new UserId(Id), loginInfo));
        }

        internal void RemoveLogin(UserLoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("User does not exists");

            RaiseEvent(new UserLoginRemoved(Guid.NewGuid(), DateTime.Now, new UserId(Id), loginInfo));
        }


        internal void AddPassword(string password)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("User does not exists");

            // todo controllo che la vecchia pwd sia nulla
            RaiseEvent(new UserPasswordChanged(Guid.NewGuid(), DateTime.Now, new UserId(Id), password));
        }

        internal void ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(Id))
                throw new Exception("User does not exists");

            // todo controllo che la vecchia pwd sia uguale ad oldpassword
            RaiseEvent(new UserPasswordChanged(Guid.NewGuid(), DateTime.Now, new UserId(Id), newPassword));
        }


        protected override IMemento GetSnapshot()
        {
            return _state;
        }





    }

}
