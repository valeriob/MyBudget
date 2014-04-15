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
            //var excel = new ExcelQueryFactory(@"C:\Users\Valerio\Downloads\TEMP.xlsx");
            //var query = from c in excel.Worksheet<Movement>()
            //            where c.Date != DateTime.MinValue
            //            select c;

            var excel = new ExcelQueryFactory(@"C:\Users\Valerio\Downloads\spese.xlsx");
            var anni = new[] {2011, 2012, 2013, 2014 };
            var movements = new List<Movement>();
            foreach(var anno in anni )
            {
                movements.AddRange(excel.Worksheet<Movement>(anno + "").Where(r => r.Data != DateTime.MinValue));
            }
   
         
            //var query2013 = from c in excel.Worksheet<Movement>("2013")
            //            select c;

            //var query2014 = from c in excel.Worksheet<Movement>("2014")
            //            select c;
            //var movements = query2013.ToList().Concat(query2014.ToList()).Where(r => r.Data != DateTime.MinValue).ToList();
          //  var movements = query.ToList();

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

            //if (string.IsNullOrEmpty(userId))
            //{
            //    var addUser = cm.Create<AddUser>();
            //    addUser.Handle(new AddUser { });
            //}
            var importer = new ImportManager(cm, pm);
            importer.ImportCategoriesByName(movements.Select(s => s.Categoria), budgetId, userId);

            var categories = pm.GetCategories().GetBudgetsCategories(budgetId);

            var createLine = cm.Create<CreateLine>();
            foreach (var m in movements)
                createLine(m.ToCreateLine(new BudgetId(budgetId), userId, categories));

        }


    }

    public class Movement
    {
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
        public string Descrizione { get; set; }
        public decimal Spesa { get; set; }

        public CreateLine ToCreateLine(BudgetId budgetId, string userId, IEnumerable<MyBudget.Projections.Category> categories)
        {
            var category =  Categoria.Trim().Replace((char)160, ' ');
            var categoryId = categories.FirstOrDefault(d => string.Compare(d.Name, category, true) == 0).Id;

            return new CreateLine
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Amount = new Amount(Currencies.Euro(), Convert.ToDecimal(Spesa)),
                BudgetId = budgetId.ToString(),
                CategoryId = categoryId,
                Date = Data,
                Description = Descrizione,
                LineId = LineId.Create(budgetId).ToString(),
                UserId = userId,
            };
        }

        public override string ToString()
        {
            return string.Format("{3} il {0:d} : {1} - {2}", Data, Categoria, Descrizione, Spesa);
        }
    }
}
