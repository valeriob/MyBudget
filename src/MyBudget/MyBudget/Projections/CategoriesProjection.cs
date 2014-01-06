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
    public interface ICategoriesProjection
    {
        IEnumerable<string> GetBudgetsCategories(BudgetId budgetId);
    }

    public class CategoriesProjection : InMemoryProjection, ICategoriesProjection
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
            AddCategory(evnt.BudgetId.ToString(), evnt.Category);
        }

        void When(LineExpenseChanged evnt)
        {
            AddCategory(evnt.BudgetId.ToString(), evnt.Category);
        }

        void AddCategory(string budget, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return;
            List<string> categories;
            if (_categories.TryGetValue(budget, out categories) == false)
                _categories[budget] = categories = new List<string>();

            if (categories.Any(c => string.Compare(c, category, true) == 0) == false)
                categories.Add(category);
        }

        public IEnumerable<string> GetBudgetsCategories(BudgetId budgetId)
        {
            List<string> categories;
            if (_categories.TryGetValue(budgetId.ToString(), out categories) == false)
                _categories[budgetId.ToString()] = categories = new List<string>();
            return categories.OrderBy(d => d);
        }

    }
}
