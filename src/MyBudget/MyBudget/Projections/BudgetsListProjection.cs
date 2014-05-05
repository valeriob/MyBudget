using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Users;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public interface IBudgetsListProjection
    {
        IEnumerable<Budget> GetBudgetsUserCanView(UserId userId);
        Budget GetBudgetById(BudgetId budgetId);
    }

    public class BudgetsListProjection : InMemoryProjection, IBudgetsListProjection
    {
        Dictionary<string, Budget> _budgets = new Dictionary<string, Budget>();
        Dictionary<string, string> _userNames = new Dictionary<string, string>();


        public BudgetsListProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
            : base(endpoint, credentials, adapter, streamName)
        {
        }


        protected override void Dispatch(dynamic evnt)
        {
            try
            {
                ((dynamic)this).When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }


        void When(BudgetCreated evnt)
        {
            if (_budgets.ContainsKey(evnt.BudgetId.ToString()))
                return;

            //var userName = _userNames[evnt.Owner.ToString()];
            var userName = "NA";

            var s = new Budget();
            s.Apply(evnt, userName);
            _budgets.Add(evnt.BudgetId.ToString(), s);
        }

        void When(UserCreated evnt)
        {
            if (_userNames.ContainsKey(evnt.UserId.ToString()))
                return;
            _userNames[evnt.UserId.ToString()] = evnt.UserName;
        }

        void When(BudgetAccessAllowed evnt)
        {
            _budgets[evnt.BudgetId.ToString()].Apply(evnt);
        }

        void When(BudgetDistributionKeyCreated evnt)
        {
            _budgets[evnt.BudgetId.ToString()].Apply(evnt);
        }


        public IEnumerable<Budget> GetBudgetsUserCanView(UserId userId)
        {
            return _budgets.Values.Where(b => b.CanRead(userId));
        }

        public Budget GetBudgetById(BudgetId budgetId)
        {
            return _budgets[budgetId.ToString()];
        }

    }

    public class Budget
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string OwnerId { get; set; }
        public string OwnerUsername { get; set; }

        HashSet<string> _allowedUsers = new HashSet<string>();
        HashSet<string> _keys = new HashSet<string>();

        internal void Apply(BudgetCreated evnt, string ownerUsername)
        {
            Id = evnt.BudgetId.ToString();
            Name = evnt.Name;
            OwnerId = evnt.Owner.ToString();
            Created = evnt.Timestamp;
            OwnerUsername = ownerUsername;

            _allowedUsers.Add(OwnerId);
        }


        internal void Apply(BudgetAccessAllowed evnt)
        {
            _allowedUsers.Add(evnt.AllowedUserId.ToString());
        }

        internal bool CanRead(UserId userId)
        {
            return _allowedUsers.Contains(userId.ToString());
        }

        internal void Apply(BudgetDistributionKeyCreated evnt)
        {
            _keys.Add(evnt.Name);
        }

        public IEnumerable<string> GetDistributionKeys()
        {
            return _keys;
        }
    }

}
