﻿using EventStore.ClientAPI;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBudgetImporter
{
    class Program
    {
        static string budgetId = "Budgets-89fb59a1_45ff_47d3_bf32_03b9005b73ca";
        static string userId = "Users-3e0d7fe4_adf6_40fd_9f68_0949ba7cf52c";

        static string _cs = "Data Source=vborioni.cloudapp.net,1433;Initial Catalog=HomeBudget;Integrated Security=False;User ID=vborioni;Password=onit!2013;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        static string query = @"select m.DateTime, m.Import, m.ShortDescription, c.Name from  Movements m
join Categories c on m.CategoryId = c.Id
where c.BudgetId = 1
and Direction = 'Out'
and Deleted = 0";

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("it-IT");
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var esCon = EventStoreConnection.Create(endpoint);
            esCon.Connect();

            var credentials = new EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit");

            var cm = new CommandManager(esCon);
            var pm = new MyBudget.Projections.ProjectionManager(endpoint, credentials, new MyBudget.Infrastructure.EventStoreAdapter(endpoint, credentials));
            pm.Run();

            var tu = pm.GetUsersList().AllUsers();
            tu.Wait();
            userId = tu.Result.Select(s => s.Id).FirstOrDefault();
            budgetId = pm.GetBudgetsList().GetBudgetsUserCanView(new MyBudget.Domain.Users.UserId(userId)).Select(s => s.Id).FirstOrDefault();

            using (var con = new System.Data.SqlClient.SqlConnection(_cs))
            {
                con.Open();
                var movements = LoadMovements(con);

                foreach (var anno in movements.GroupBy(g=> g.DateTime.Year))
                {
                    
                    var str = ServiceStack.Text.CsvSerializer.SerializeToCsv(anno.OrderBy(d=> d.DateTime));
                    System.IO.File.WriteAllText(@"c:\temp\Year_"+anno.Key + ".csv", str);
                }
                var importer = new ImportManager(cm, pm);
                importer.ImportCategoriesByName(movements.Select(s => s.Category), budgetId, userId);

                var categories = pm.GetCategories().GetBudgetsCategories(budgetId);

                var handler = cm.Create<CreateLine>();
                foreach (var m in movements)
                    handler(m.ToCreateLine(new BudgetId(budgetId), userId, categories));
            }
        }

        static IEnumerable<Movement> LoadMovements(IDbConnection con)
        {
            var result = new List<Movement>();
            using (var cmd = Prepare(con, query))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                {
                    var mov = reader.ToMovement();
                    result.Add(mov);
                }
            return result;
        }

        static IDbCommand Prepare(IDbConnection con, string query, IEnumerable<Tuple<string, object>> parameters = null)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = query;
            if (parameters != null)
                foreach (var p in parameters)
                {
                    var par = cmd.CreateParameter();
                    par.Value = p.Item2;
                    par.ParameterName = p.Item1;
                    cmd.Parameters.Add(par);
                }
            return cmd;
        }

    }


    public static class DataReaderExtensions
    {
        public static Movement ToMovement(this IDataReader reader)
        {
            return new Movement
            {
                DateTime = reader.GetDateTime(0).Date,
                FormattedDate = reader.GetDateTime(0).Date.ToString("dd/MM/yyyy"),
                Import = reader.GetDouble(1),
                ShortDescription = reader.GetString(2),
                Category = reader.GetString(3)
            };
        }

        public static CreateLine ToCreateLine(this Movement mov, BudgetId budgetId, string userId, IEnumerable<MyBudget.Projections.Category> categories)
        {
            var category = mov.Category.Trim().Replace((char)160, ' ');
            var categoryId = categories.FirstOrDefault(d => string.Compare(d.Name, category, true) == 0).Id;

            var expense = new Expense(new Amount(Currencies.Euro(), Convert.ToDecimal(mov.Import)), mov.DateTime, categoryId, mov.ShortDescription);


            return new CreateLine
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                BudgetId = budgetId.ToString(),
                LineId = LineId.Create(budgetId).ToString(),
                UserId = userId,
                Expense = expense,
            };
        }
    }

    public class Movement
    {
        public double Import { get; set; }
        public DateTime DateTime { get; set; }
        public string ShortDescription { get; set; }
        public string Category { get; set; }
        public string FormattedDate { get; set; }
    }

}
