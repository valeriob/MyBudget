using CommonDomain;
using CommonDomain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.Accounts
{
    public class AccountState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }
    }

    public class AccountId
    {
        string _id;
        public AccountId(string id)
        {
            _id = id;
        }
        public AccountId()
        {
            _id = "Account-" + Guid.NewGuid();
        }
    }

    public class AccountCreated :  Event
    {

    }

    public class Account : AggregateBase
    {
        AccountState _state;
        public Account(AccountState state)
        {
            _state = state;
        }

        public Account()
            : this(new AccountState())
        {

        }

        public void Create(AccountId id)
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
