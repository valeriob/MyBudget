using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actor
{
    public class BaseActor
    {

    }
    public class TestActor : BaseActor
    {

    }

    public class ActorRef
    {
        string _id;
        public ActorRef(string id)
        {
            _id = id;
        }

        public void Enqueue(Message msg, IEventStoreConnection connection, string queue)
        {
            //todo
            var evnt = new EventData(msg.Id, "type", true, null, null);
            connection.AppendToStream(queue, ExpectedVersion.Any, evnt);
        }

        //string InputQueue()
        //{
        //    return _id + "-Input";
        //}
    }


    public class ActorContext
    {
        IEventStoreConnection _connection;
        string _queue;

        public ActorContext(IEventStoreConnection connection, string queue)
        {
            _connection = connection;
            _queue = queue;
        }


       

        public void Deliver(Message msg)
        {
            var actorRef = GetRef(msg.ActorId);
            actorRef.Enqueue(msg, _connection, _queue);
        }

        ActorRef GetRef(string id)
        {
            return new ActorRef(id);
        }

    }

    public class ActorRunner
    {
        SubscriptionRef _subscription;
        IEventStoreConnection _connection;
        public ActorRunner(SubscriptionRef subscription, IEventStoreConnection connection)
        {
            _connection = connection;
            _subscription = subscription;
        }

        public void Start()
        {
            //_connection.SubscribeToStreamFrom()
        }
    }

    public class SubscriptionRef
    {
        string _name;
        public SubscriptionRef(string name)
        {
            _name = name;
        }

        public EventStoreCatchUpSubscription Subscribe(IEventStoreConnection connection, Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared)
        {
            var slice = connection.ReadStreamEventsBackward(GetCommittedStream(),0, 1, true);

            //slice.Events.
            return connection.SubscribeToStreamFrom(_name, null, true, eventAppeared);
        }
        string GetCommittedStream()
        {
            return _name + "-Commit";
        }
    }

    public class Message
    {
        public Guid Id { get; set; }
        public string ActorId { get; set; }
    }
}
