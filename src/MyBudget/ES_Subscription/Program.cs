using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget;
using MyBudget.Commands;
using MyBudget.Domain.ValueObjects;
using MyBudget.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ES_Subscription
{
    class Program
    {
        static IPEndPoint address;
        static UserCredentials userCredentials;
        static ManualResetEvent mre = new ManualResetEvent(false);
        static int count;
        static EventStoreAdapter _adapter;

        static void Main(string[] args)
        {
            userCredentials = new UserCredentials("admin", "changeit");

            address = new IPEndPoint(IPAddress.Loopback, 1113);
            var con = EventStoreConnection.Create(address);
            con.Connect();

            _adapter = new EventStoreAdapter(address, userCredentials);
            //CreateLines(1000000, con, "Users-5e3191f5_bb94_4074_b46b_9d4f8e4461b2");
            MeasureLines(con, userCredentials);
            // FromAll(con, userCredentials);
            //FromStream(con, userCredentials);
        }
        static void MeasureLines(IEventStoreConnection con, UserCredentials credentials)
        {
            var stream = "lines_of_Budgets-a208105f_d5ba_475e_bb23_e7012945d332";
            var w = Stopwatch.StartNew();

            var sub = con.SubscribeToStreamFrom(stream, null, true, Appeared, Live, null, userCredentials);

            mre.WaitOne();
            w.Stop();

            Console.WriteLine("read {0} events, in {1}. {2}", count, w.Elapsed, count / w.Elapsed.TotalSeconds);
            Console.ReadLine();
        }
        static void CreateLines(int numberOfLines, IEventStoreConnection esCon, string userId)
        {
            var cm = new CommandManager(esCon);
    
            var budgetId = MyBudget.Domain.Budgets.BudgetId.Create();
            var currency = Currencies.Euro();
            var random = new Random();
            var categories = new[] { "C1", "C2", "C3", "C4" };
            var mindate = new DateTime(2010, 1, 1);

            //var cb = cm.Create<CreateBudget>();
            //cb.Handle(new CreateBudget
            //{
            //    BudgetId = budgetId.ToString(),
            //    UserId = userId,
            //    BudgetName = "test 1. #" + numberOfLines
            //});

            var cl = cm.Create<CreateLine>();
            var po = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            Parallel.For(0, numberOfLines,po, i =>
            //for (int i = 0; i < numberOfLines; i++)
            {
                cl.Handle(new CreateLine
                {
                    UserId = userId,
                    BudgetId = budgetId.ToString(),
                    Amount = new Amount(currency, random.Next(100)),
                    CategoryId = categories[random.Next(categories.Length)],
                    Date = mindate.AddDays(random.Next(365 * 3)),
                    Description = "nothing special",
                    LineId = MyBudget.Domain.Lines.LineId.Create(budgetId).ToString(),
                });
            }
            );
        }

        static void FromStream(IEventStoreConnection con, UserCredentials userCredentials)
        {
            var ad = new MyBudget.Infrastructure.EventStoreAdapter(address, userCredentials);

            // var sub = con.SubscribeToStreamFrom("$category-Users", null, true, Appeared, Live, Dropped, userCredentials);
            //   sub.Start();

            var read = con.ReadStreamEventsForward("lines_of_", 0, 1000, true, userCredentials);

            foreach (var e in read.Events)
            {
                var t = ad.TryGetDomainEvent(e);

            }
            var mre = new ManualResetEvent(false);
            mre.WaitOne(3000);

            var rgpsa = events.GroupBy(g => g.Event.EventId).ToList();
            int i = 0;
            var rgps = events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();
            //var rgps2 = read.Events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();

            foreach (var r in rgps)
            {
                var values = r.ToArray();

                foreach (var v in values)
                {

                    var t = ad.TryGetDomainEvent(v.s);
                }
            }


            Console.ReadLine();
        }

        static void FromAll(IEventStoreConnection con, UserCredentials userCredentials)
        {
            var sub = con.SubscribeToAllFrom(Position.Start, true, Appeared, Live, Dropped, userCredentials);
            // sub.Start();

            var read = con.ReadAllEventsForward(Position.Start, 1000, true, userCredentials);
            var mre = new ManualResetEvent(false);
            mre.WaitOne(3000);

            var rgpsa = events.GroupBy(g => g.Event.EventId).ToList();
            int i = 0;
            var rgps = events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();
            var rgps2 = read.Events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();

            foreach (var r in rgps)
            {
                var values = r.ToArray();

            }
            Console.ReadLine();
        }

        private static void Live(EventStoreCatchUpSubscription obj)
        {
            mre.Set();
        }

        private static void Dropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason arg2, Exception arg3)
        {

        }

        static List<ResolvedEvent> events = new List<ResolvedEvent>();

        private static void Appeared(EventStoreCatchUpSubscription arg1, ResolvedEvent evnt)
        {
            _adapter.TryGetDomainEvent(evnt);

            count++;
            if(count % 1000 == 0)
                Console.WriteLine("{0}", count);
            //events.Add(arg2);
        }
    }
}
