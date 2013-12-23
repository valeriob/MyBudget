using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets.ValueObjects
{
    public sealed class UserLoginInfo
    {
        public UserLoginInfo(string loginProvider, string providerKey)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }

        public string LoginProvider { get; private set; }
        public string ProviderKey { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as UserLoginInfo;
            return other != null && other.LoginProvider == LoginProvider && other.ProviderKey == ProviderKey;
        }
    }
}
