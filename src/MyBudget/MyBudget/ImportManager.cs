using MyBudget.Commands;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget
{
    public class ImportManager
    {
        CommandManager _commandManager;
        ProjectionManager _projectionManager;

        public ImportManager(CommandManager cm, ProjectionManager pm)
        {
            _commandManager = cm;
            _projectionManager = pm;
        }


        public void ImportCategoriesByName(IEnumerable<string> categoryNames, string budgetId, string userId)
        {
            var cp = _projectionManager.GetCategories();
            var createCategory = _commandManager.Create<CreateCategory>();

            DateTime lastUpdate = DateTime.MinValue;
            var cleaned = categoryNames.Select(s => s.Trim().Replace((char)160, ' ')).Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(d=> d)
                .ToList();

            foreach (var categoryName in cleaned)
            {
                var task = cp.GetBudgetsCategories(budgetId, lastUpdate);
                task.Wait();
                if (task.Result.Any(r => string.Compare(r.Name, categoryName, true) == 0) == false)
                    createCategory(new CreateCategory
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = lastUpdate = DateTime.Now,
                        BudgetId = budgetId,
                        CategoryId = "Category-" + Guid.NewGuid(),
                        UserId = userId,
                        CategoryDescription = "",
                        CategoryName = categoryName
                    });
            }

            //cp.GetBudgetsCategories(budgetId, lastUpdate).Wait();
        }
    }
}
