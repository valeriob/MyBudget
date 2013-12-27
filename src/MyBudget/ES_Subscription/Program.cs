using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ES_Subscription
{
    class Program
    {
        static void Main(string[] args)
        {
            var userCredentials = new UserCredentials("admin", "changeit");

            var address = new IPEndPoint(IPAddress.Loopback, 1113);
            var con = EventStoreConnection.Create(address);
            con.Connect();

           // FromAll(con, userCredentials);
            FromStream(con, userCredentials);
        }

        static void FromStream(IEventStoreConnection con, UserCredentials userCredentials)
        {
            var sub = con.SubscribeToStreamFrom("$category-Users", null, true, Appeared, Live, Dropped, userCredentials);
            sub.Start();

            //var read = con.ReadStreamEventsForward("$category-Users", 0, 1000, true, userCredentials);
            var mre = new ManualResetEvent(false);
            mre.WaitOne(3000);

            var rgpsa = events.GroupBy(g => g.Event.EventId).ToList();
            int i = 0;
            var rgps = events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();
            //var rgps2 = read.Events.Select(s => new { s, position = i++ }).GroupBy(g => g.s.Event.EventId).ToList();

            foreach (var r in rgps)
            {
                var values = r.ToArray();

            }
            Console.ReadLine();
        }

        static void FromAll(IEventStoreConnection con, UserCredentials userCredentials)
        {
            var sub = con.SubscribeToAllFrom(Position.Start, true, Appeared, Live, Dropped, userCredentials);
            sub.Start();

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
            
        }

        private static void Dropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason arg2, Exception arg3)
        {
            
        }

        static List<ResolvedEvent> events = new List<ResolvedEvent>();

        private static void Appeared(EventStoreCatchUpSubscription arg1, ResolvedEvent arg2)
        {
            events.Add(arg2);
        }
    }
}
