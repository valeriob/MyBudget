using EventStore.ClientAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Actor;


// local queue

namespace Actor.P2
{
    public class Message
    {
        public Guid Id { get; set; }
        public string ActorId { get; set; }
    }
    public class Event : Message
    {
        public int InputVersion { get; set; }
        public int Version { get; set; }
    }

    //public class Command : Message
    //{

    //}

    public class BaseActor
    {
        public string Id { get; set; }
        public int InputVersion { get; set; }
        public int Version { get; set; }

        public BaseActor(IEnumerable<Event> events)
        {

        }

        public void Handle(Message msg, int version)
        {
            if (InputVersion >= version)
                throw new OptimisticConcurrencyException();
        }

    }
    public class TestActor : BaseActor
    {
        public TestActor(IEnumerable<Event> events) : base(events)
        {

        }
    }

    public class ActorContext
    {
        IEventStoreConnection _connection;
        EventStoreAllCatchUpSubscription _subscription;
        ActorWorker[] _workers;

        public ActorContext(IEventStoreConnection connection, int workers = 1)
        {
            _connection = connection;
            _workers = new ActorWorker[workers];

            for (int i = 0; i < workers; i++)
                _workers[i] = new ActorWorker(connection);
        }


        public void Start()
        {
            _subscription = _connection.SubscribeToAllFrom(Position.Start, true, EventAppeared);
        }

        public void Deliver(Message msg)
        {
            var evnt = new EventData(msg.Id, "type", true, null, null);
            _connection.AppendToStream(msg.ActorId + "-input", ExpectedVersion.Any, evnt);
        }

        void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
        {
            //evnt.Event.Data
            Message msg = null;

            var worker = FindWorker(msg.ActorId);
            worker.EnqueueKick(msg.ActorId);
        }

        ActorWorker FindWorker(string actorId)
        {
            // consistent hash
            return _workers.First();
        }

    }

    public class ActorWorker
    {
        BlockingCollection<string> _queue;
        bool _running;
        IEventStoreConnection _connection;
        int _maxCapacity;

        public ActorWorker(IEventStoreConnection connection)
        {
            _maxCapacity = 1000;
            _queue = new BlockingCollection<string>(new ConcurrentQueue<string>(), _maxCapacity);
            _connection = connection;
            _running = true;
            ThreadPool.QueueUserWorkItem(Run);
        }

        public void EnqueueKick(string actorId)
        {
            _queue.Add(actorId);
        }


        void Run(object state)
        {
            while (_running)
            {
                string actorId = null;
                if (_queue.TryTake(out actorId, 10))
                {
                    Kick(actorId);
                }
                else
                    Thread.Sleep(1);
            }
        }


        void Kick(string actorId)
        {
            var actor = FindActor(actorId);
            var command = GetInputMessage(actorId, actor.InputVersion);
            
            // deserialize
            Message msg = null;
            
            try
            {
                actor.Handle(msg, command.Event.EventNumber);

                SaveActor(actor);
            }
            catch(OptimisticConcurrencyException)
            {

            }
            catch
            {
                Dead(msg);
            }
           
        }

        BaseActor FindActor(string actorId)
        {
            var slice = _connection.ReadStreamEventsForward(actorId, ExpectedVersion.Any, int.MaxValue, true);
            var events = ToEvents(slice.Events);
            return new BaseActor(events);
        }

        IEnumerable<Event> ToEvents(IEnumerable<ResolvedEvent> events)
        {
            return Enumerable.Empty<Event>();
        }

        void SaveActor(BaseActor actor)
        {
            // extract events from actor
            // serialize events
            var events = new EventData[2];
            _connection.AppendToStream(actor.Id, ExpectedVersion.Any, events);
        }

        void Dead(Message msg)
        {
            // todo
            var evnt = new EventData(msg.Id, "type", true, null, null);
            _connection.AppendToStream(msg.ActorId + "-dead", ExpectedVersion.Any, evnt);
        }

        ResolvedEvent GetInputMessage(string actorId, int minVersion)
        {
            var slice = _connection.ReadStreamEventsForward(actorId + "-input", minVersion, 1, true);
            return slice.Events.Single();
        }
    }
    
}
