using EventStore.ClientAPI;
using LinqToExcel;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
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
        static string budgetId = "Budgets-22d5c5d0_8eef_459a_8224_269292b55d2e";
        static string userId = "Users-5e3191f5_bb94_4074_b46b_9d4f8e4461b2";

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

            var handler = cm.Create<CreateLine>();
            foreach (var m in movements)
                handler.Handle(m.ToCreateLine(new BudgetId(budgetId), userId));

        }
    }

    public class Movement
    {
        public DateTime Date { get; set; }
        public string Category{ get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public CreateLine ToCreateLine(BudgetId budgetId, string userId)
        {
            return new CreateLine
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Amount = new Amount(Currencies.Euro(), Convert.ToDecimal(Amount)),
                BudgetId = budgetId.ToString(),
                Category = Category.Trim().Replace((char)160,' '),
                Date = Date,
                Description = Description,
                LineId = LineId.Create(budgetId).ToString(),
                UserId = userId,
            };
        }
    }
}
