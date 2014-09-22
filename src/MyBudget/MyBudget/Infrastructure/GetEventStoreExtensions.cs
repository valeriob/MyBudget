using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStore.ClientAPI
{
    public static class GetEventStoreExtensions
    {
        public static void Connect(this IEventStoreConnection con)
        {
            var task = con.ConnectAsync();
            task.Wait();
        }

        public static StreamEventsSlice ReadStreamEventsForward(this IEventStoreConnection con, string stream, int start, int count, bool resolveLinkTos, UserCredentials userCredentials = null)
        {
            var task = con.ReadStreamEventsForwardAsync(stream, start, count, resolveLinkTos, userCredentials);
            task.Wait();
            return task.Result;
        }

        public static AllEventsSlice ReadAllEventsForward(this IEventStoreConnection con, Position position, int maxCount, bool resolveLinkTos, UserCredentials userCredentials = null)
        {
            var task = con.ReadAllEventsForwardAsync(position, maxCount, resolveLinkTos, userCredentials);
            task.Wait();
            return task.Result;
        }
        

        public static WriteResult AppendToStream(this IEventStoreConnection con, string stream, int expectedVersion, IEnumerable<EventData> events, UserCredentials userCredentials = null)
        {
            var task = con.AppendToStreamAsync(stream, expectedVersion, events, userCredentials);
            task.Wait();
            return task.Result;
        }

    


        public static EventStoreTransaction StartTransaction(this IEventStoreConnection con, string stream, int expectedVersion, UserCredentials userCredentials = null)
        {
            var task = con.StartTransactionAsync(stream, expectedVersion, userCredentials);
            task.Wait();
            return task.Result;
        }

        public static void Write(this EventStoreTransaction tx, IEnumerable<EventData> events)
        {
            var task = tx.WriteAsync(events);
            task.Wait();
        }

        public static WriteResult Commit(this EventStoreTransaction tx)
        {
            var task = tx.CommitAsync();
            task.Wait();
            return task.Result;
        }

    }

}
