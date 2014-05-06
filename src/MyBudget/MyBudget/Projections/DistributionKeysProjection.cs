using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
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
    public interface IDistributionKeysProjection
    {
        IEnumerable<DistributionKey> GetBudgetsDistributionKeys(string budgetId);
        Task<IEnumerable<DistributionKey>> GetBudgetsDistributionKeys(string budgetId, DateTime updated);
    }

    public class DistributionKeysProjection : InMemoryProjection, IDistributionKeysProjection
    {
        Dictionary<string, List<DistributionKey>> _keys = new Dictionary<string, List<DistributionKey>>();


        public DistributionKeysProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
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


        void When(DistributionKeyCreated evnt)
        {
            AddDistributionKey(evnt.BudgetId, evnt.DistributionKeyId, evnt.Name, evnt.Description);
        }

        void When(DistributionKeyUpdated evnt)
        {
            UpdateDistributionKey(evnt.BudgetId, evnt.DistributionKeyId, evnt.Name, evnt.Description);
        }

        void AddDistributionKey(string budget, string id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            List<DistributionKey> DistributionKeys;
            if (_keys.TryGetValue(budget, out DistributionKeys) == false)
                _keys[budget] = DistributionKeys = new List<DistributionKey>();

            if (DistributionKeys.Any(c => c.Id == id) == false)
                DistributionKeys.Add(new DistributionKey { Id = id, Name = name, Description = description });
        }

        void UpdateDistributionKey(string budget, string id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            List<DistributionKey> DistributionKeys;
            if (_keys.TryGetValue(budget, out DistributionKeys) == false)
                _keys[budget] = DistributionKeys = new List<DistributionKey>();

            var DistributionKey = DistributionKeys.Single(s => s.Id == id);
            DistributionKey.Name = name;
            DistributionKey.Description = description;
        }

        public IEnumerable<DistributionKey> GetBudgetsDistributionKeys(string budgetId)
        {
            if (HasLoaded == false)
                throw new Exception("Not loaded");

            List<DistributionKey> DistributionKeys;
            if (_keys.TryGetValue(budgetId.ToString(), out DistributionKeys) == false)
                _keys[budgetId] = DistributionKeys = new List<DistributionKey>();
            return DistributionKeys.OrderBy(d => d.Name);
        }

        public async Task<IEnumerable<DistributionKey>> GetBudgetsDistributionKeys(string budgetId, DateTime updated)
        {
            while (LastUpdate < updated || HasLoaded == false)
                await Task.Delay(100);

            return GetBudgetsDistributionKeys(budgetId);
        }
    }

    public class DistributionKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
