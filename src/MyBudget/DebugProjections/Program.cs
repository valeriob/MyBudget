using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget;
using MyBudget.Domain.Lines;
using MyBudget.Infrastructure;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DebugProjections
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var esCon = EventStoreConnection.Create(endpoint);
            esCon.Connect();

            var credentials = new UserCredentials("admin", "changeit");

            var adapter = new EventStoreAdapter(endpoint, credentials);

            //var cm = new CommandManager(esCon);
            //var pm = new ProjectionManager(endpoint, credentials, adapter);
            //pm.Run();


            //var cp = new MyDiagProjection(endpoint, credentials, adapter, "Budgets-1722f4ea_a9a5_4d97_8c39_c9d450a1331a");
            //cp.Start();
            //while (cp.HasLoaded == false)
            //    System.Threading.Thread.Sleep(100);

            //Console.WriteLine("done from all");
            //Console.ReadLine();

            var cp2 = new BudgetLinesProjection("Budgets-1722f4ea_a9a5_4d97_8c39_c9d450a1331a", endpoint, credentials, adapter, "lines_of_Budgets-1722f4ea_a9a5_4d97_8c39_c9d450a1331a" );
            cp2.Start();

            //var cp2 = new MyDiagProjection(endpoint, credentials, adapter, "Budgets-1722f4ea_a9a5_4d97_8c39_c9d450a1331a", "lines_of_Budgets-1722f4ea_a9a5_4d97_8c39_c9d450a1331a");
            //cp2.Start();
            while (cp2.HasLoaded == false)
                System.Threading.Thread.Sleep(100);
            
            Console.ReadLine();
            //var events = adapter.GetStreamEvents("");
            
        }
    }

    public class MyDiagProjection : InMemoryProjection
    {
        string _budgetId;
        List<BudgetLine> _lines = new List<BudgetLine>();

        public MyDiagProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string budgetId) : base(endpoint, credentials, adapter)
        {
            _budgetId = budgetId;
        }

        public MyDiagProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string budgetId, string streamName)
            : base(endpoint, credentials, adapter, streamName)
        {
            _budgetId = budgetId;
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
            if (evnt.BudgetId.ToString().Equals(_budgetId) == false)
                return;

            if (_lines.Any(l => l.Id == evnt.LineId.ToString()))
                return;
            _lines.Add(new BudgetLine(evnt));
        }
    }
}
