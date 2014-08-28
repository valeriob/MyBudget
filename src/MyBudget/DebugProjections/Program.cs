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

            var cm = new CommandManager(esCon);
            var pm = new ProjectionManager(endpoint, credentials, adapter);
            pm.Run();


            var cp = new MyDiagProjection(endpoint, credentials, adapter, "Budgets-d46879f5_8130_4afb_b947_ecc0021e50be");
            cp.Start();
            while (cp.HasLoaded == false)
                System.Threading.Thread.Sleep(100);


            var cp2 = new MyDiagProjection(endpoint, credentials, adapter, "Budgets-d46879f5_8130_4afb_b947_ecc0021e50be", "lines_of_Budgets-d46879f5_8130_4afb_b947_ecc0021e50be");
            cp2.Start();
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
