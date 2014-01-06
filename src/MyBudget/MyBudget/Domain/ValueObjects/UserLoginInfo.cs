using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public sealed class UserLoginInfo
    {
        public UserLoginInfo(string loginProvider, string providerKey)
        {
            if (string.IsNullOrEmpty(loginProvider))
                throw new ArgumentNullException("loginProvider");
            if (string.IsNullOrEmpty(providerKey))
                throw new ArgumentNullException("providerKey");

            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }

        public string LoginProvider { get; private set; }
        public string ProviderKey { get; private set; }

        public override int GetHashCode()
        {
            return 3 + 5 * LoginProvider.GetHashCode() + 7 * ProviderKey.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var other = obj as UserLoginInfo;
            return other != null && other.LoginProvider == LoginProvider && other.ProviderKey == ProviderKey;
        }
    }
}
