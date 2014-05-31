using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
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
        static void Main(string[] args)
        {
            string file = @"c:\Users\vborioni.ONIT\Downloads\spese.xlsx";

            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var esCon = EventStoreConnection.Create(endpoint);
            esCon.Connect();

            var credentials = new UserCredentials("admin", "changeit");

            var cm = new CommandManager(esCon);
            var pm = new MyBudget.Projections.ProjectionManager(endpoint, credentials, new EventStoreAdapter(endpoint, credentials));
            pm.Run();

            var context = new BudgetChooser(pm).ChooseBudget();

            file = @"C:\Users\Valerio\Downloads\Spese Laura e Valerio.xlsx";
            new ImportDistribution(pm, cm).Run(context.BudgetId, context.UserId, file); 

            new ImportStandard(pm, cm).Run(context.BudgetId, context.UserId, file);
        }


    }
}
