using EventStore.ClientAPI;
using LinqToExcel;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImporter
{
    public class ImportStandard
    {
        ProjectionManager _pm;
        CommandManager _cm;

        public ImportStandard(ProjectionManager pm, CommandManager cm)
        {
            _cm = cm;
            _pm = pm;
        }

        public void Run(string budgetId, string userId, string file)
        {
            var excel = new ExcelQueryFactory(file);
            var anni = new[] { 2011, 2012, 2013, 2014 };
            var movements = new List<Movimento>();
            foreach (var anno in anni)
            {
                movements.AddRange(excel.Worksheet<Movimento>(anno + "").Where(r => r.Data != DateTime.MinValue));
            }
            movements = movements.OrderBy(d => d.Data).ToList();

            Console.WriteLine("Read {0} movements from {1}", movements.Count, file);

            var importer = new ImportManager(_cm, _pm);
            var categorie = movements.Select(s => s.Categoria).Where(r => r != "Arancio").ToArray();

            importer.ImportCategoriesByName(categorie, budgetId, userId);

            var categories = _pm.GetCategories().GetBudgetsCategories(budgetId);

            var createLine = _cm.Create<CreateLine>();
            DateTime last = DateTime.MinValue;
            foreach (var m in movements)
            {
                last = DateTime.Now;
                createLine(m.ToCreateLine(new BudgetId(budgetId), userId, categories));
            }
            var bp = _pm.GetBudgetLinesProjection(budgetId);
            var galt = bp.GetAllLines(last);
            galt.Wait();
            var lines = galt.Result;

            Console.WriteLine("Loaded {0} movements into {1}", lines.Count(), budgetId);
        }
    }
}
