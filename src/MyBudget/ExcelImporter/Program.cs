using EventStore.ClientAPI;
using LinqToExcel;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImporter
{
    class Program
    {
        static string budgetId = "Budgets-16275c2a_5b53_47e5_af04_f515668ae0f5";
        static string userId = "Users-4d0d6ff0_c45a_4414_8f01_f200e28b70b5";

        static void Main(string[] args)
        {
            var excel = new ExcelQueryFactory(@"C:\Users\Valerio\Downloads\TEMP.xlsx");
            var query = from c in excel.Worksheet<Movement>()
                        select c;
            var movements = query.ToList();

            Console.ReadLine();

            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var esCon = EventStoreConnection.Create(endpoint);
            esCon.Connect();

            var credentials = new EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit");

            var cm = new CommandManager(esCon);
            var pm = new MyBudget.Projections.ProjectionManager(endpoint, credentials, new EventStoreAdapter(endpoint, credentials));
            pm.Run();

            var tu = pm.GetUsersList().AllUsers();
            tu.Wait();
            userId = tu.Result.Select(s => s.Id).FirstOrDefault();
            budgetId = pm.GetBudgetsList().GetBudgetsUserCanView(new MyBudget.Domain.Users.UserId(userId)).Select(s => s.Id).FirstOrDefault();

            var importer = new ImportManager(cm, pm);
            importer.ImportCategoriesByName(movements.Select(s => s.Category.Trim().Replace((char)160, ' ')).Distinct().ToList(), budgetId, userId);

            var categories = pm.GetCategories().GetBudgetsCategories(budgetId);

            var mh = cm.Create<CreateLine>();
            foreach (var m in movements)
                mh.Handle(m.ToCreateLine(new BudgetId(budgetId), userId, categories));

        }
    }

    public class Movement
    {
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public CreateLine ToCreateLine(BudgetId budgetId, string userId, IEnumerable<MyBudget.Projections.Category> categories)
        {
            var category =  Category.Trim().Replace((char)160, ' ');
            var categoryId = categories.FirstOrDefault(d => string.Compare(d.Name, category, true) == 0).Id;

            return new CreateLine
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Amount = new Amount(Currencies.Euro(), Convert.ToDecimal(Amount)),
                BudgetId = budgetId.ToString(),
                CategoryId = categoryId,
                Date = Date,
                Description = Description,
                LineId = LineId.Create(budgetId).ToString(),
                UserId = userId,
            };
        }
    }
}
