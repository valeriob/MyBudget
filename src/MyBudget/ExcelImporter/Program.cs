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

            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var esCon = EventStoreConnection.Create(endpoint);
            esCon.Connect();

            var credentials = new UserCredentials("admin", "changeit");

            var cm = new CommandManager(esCon);
            var pm = new MyBudget.Projections.ProjectionManager(endpoint, credentials, new EventStoreAdapter(endpoint, credentials));
            pm.Run();

            var comuni = @"C:\Users\Valerio\Downloads\Spese Laura e Valerio.xlsx";
            var context = new BudgetChooser(pm).ChooseBudget(comuni);
            new ImportDistribution(pm, cm).Run(context.BudgetId, context.UserId, comuni);

            var singole = @"c:\Users\Valerio\Downloads\spese.xlsx";
            var c2 = new BudgetChooser(pm).ChooseBudget(singole);
            new ImportStandard(pm, cm).Run(c2.BudgetId, c2.UserId, singole);
        }


    }
}
