using Actor.P1;
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
            string actor1 = "actor1";
            string queue = "mainQueue";
            var address = new IPEndPoint(IPAddress.Loopback, 1113);
            var connection = EventStore.ClientAPI.EventStoreConnection.Create(address);

            var ctx = new ActorContext(connection, queue);
            ctx.Start();
            ctx.Deliver(new Message { Id = Guid.NewGuid(), ActorId = actor1 });

            Console.ReadLine();
        }
    }
}
