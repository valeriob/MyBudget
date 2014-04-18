using Actor.P2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Actor
{
    class Program
    {
        public static void Main(string[] args)
        {
            string actor1 = "TestActor-actor2";
            string queue = "mainQueue";
            var address = new IPEndPoint(IPAddress.Loopback, 1113);

            var serializer = new JSonNet_Serializer();
            var connection = EventStore.ClientAPI.EventStoreConnection.Create(address);
            connection.Connect();

            var repository = new CommonDomain.Persistence.GetEventStore.GetEventStoreRepository(connection, address);


            var ctx = new ActorContext(connection, serializer, repository);
            ctx.Start();
            ctx.Deliver(new Message { Id = Guid.NewGuid(), ActorId = actor1 });

            Console.ReadLine();
        }
    }
}
