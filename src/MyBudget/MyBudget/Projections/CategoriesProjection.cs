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
    public class CategoriesProjection : InMemoryProjection
    {
        Dictionary<string, List<string>> _categories = new Dictionary<string, List<string>>();


        public CategoriesProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
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

        void When(LineCreated evnt)
        {
            if (_categories.ContainsKey(evnt.BudgetId.ToString()))
                return;

            List<string> categories;
            if(_categories.TryGetValue(evnt.BudgetId.ToString(), out categories) == false)
                _categories[evnt.BudgetId.ToString()] = categories = new List<string>();

            if(categories.Any(c=> string.Compare(c, evnt.Category, true) != 0 ) == false)
                categories.Add(evnt.Category);
        }


        public IEnumerable<string> GetBudgetsCategories(BudgetId budgetId)
        {
            List<string> categories;
            if (_categories.TryGetValue(budgetId.ToString(), out categories) == false)
                _categories[budgetId.ToString()] = categories = new List<string>();
            return categories;
        }

    }
}
