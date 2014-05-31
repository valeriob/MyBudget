using EventStore.ClientAPI;
using LinqToExcel;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImporter
{
    public class ImportDistribution
    {
        ProjectionManager _pm;
        CommandManager _cm;

        public ImportDistribution(ProjectionManager pm, CommandManager cm)
        {
            _cm = cm;
            _pm = pm;
        }

        public void Run(string budgetId, string userId, string file)
        {
            var excel = new ExcelQueryFactory(file);
            var anni = new[] { 2013, 2014 };
            var movements = new List<Movimento>();
            foreach (var anno in anni)
            {
                //movements.AddRange(excel.Worksheet<Movement>(anno + "")
                //    .Where(r => r.Data != DateTime.MinValue));

                var laura = excel.WorksheetRange<Movimento>("B4", "E10000", anno + "")
                     .Where(r => r.Data != DateTime.MinValue)
                     .ToList();
                laura.ForEach(m => m.DistributionKey = "Laura");

                var valerio = excel.WorksheetRange<Movimento>("G4", "J10000", anno + "")
                     .Where(r => r.Data != DateTime.MinValue)
                     .ToList();
                valerio.ForEach(m => m.DistributionKey = "Valerio");

                var comune = excel.WorksheetRange<Movimento>("L4", "O10000", anno + "")
                     .Where(r => r.Data != DateTime.MinValue)
                     .ToList();


                movements.AddRange(laura);
                movements.AddRange(valerio);
                movements.AddRange(comune);
            }

            movements = movements.OrderBy(d => d.Data).ToList();

            
            var importer = new ImportManager(_cm, _pm);
            importer.ImportCategoriesByName(movements.Select(s => s.Categoria), budgetId, userId);

            var categories = _pm.GetCategories().GetBudgetsCategories(budgetId);

            var createLine = _cm.Create<CreateLine>();
            foreach (var m in movements)
                createLine(m.ToCreateLine(new BudgetId(budgetId), userId, categories));
        }
    }
}
