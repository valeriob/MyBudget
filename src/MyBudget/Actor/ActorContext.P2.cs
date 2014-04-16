using EventStore.ClientAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Actor;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;


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

    public class BaseActor : CommonDomain.Core.AggregateBase
    {
        public int InputVersion { get; private set; }
        IRouteEvents _router;


        public void Handle<T>(T msg, int version) where T : Message
        {
            if (InputVersion > version)
                throw new OptimisticConcurrencyException();

            GetRouter().Dispatch(msg);
        }

        public void Handle(Message msg)
        {
            //RaiseEvent
        }

        IRouteEvents GetRouter()
        {
            if(_router == null)
                _router = new HandlerConventionEventRouter(true, this);
            return _router;
        }
    }

    public class TestActorState : IMemento
    {
        public string Id { get; set; }
        public int Version { get; set; }

        public void Apply(Message msg)
        {
            Id = msg.ActorId;
        }
    }
    public class TestActor : BaseActor
    {
        TestActorState _state;
        public TestActor()
        {
            _state = new TestActorState();
        }
        protected override IMemento GetSnapshot()
        {
            return _state;
        }
    }

    public class ActorContext
    {
        IEventStoreConnection _connection;
        EventStoreAllCatchUpSubscription _subscription;
        ActorWorker[] _workers;
        ISerialize _serializer;

        public ActorContext(IEventStoreConnection connection, ISerialize serializer, IRepository repository, int workers = 1)
        {
            _serializer = serializer;
            _connection = connection;
            _workers = new ActorWorker[workers];

            for (int i = 0; i < workers; i++)
                _workers[i] = new ActorWorker(connection, _serializer, repository);
        }


        public void Start()
        {
            _subscription = _connection.SubscribeToAllFrom(Position.Start, true, EventAppeared);
            _subscription.Start();
        }

        public void Deliver(Message msg)
        {
            var actorId = msg.ActorId;

            var json = _serializer.Serialize(msg);
            var data = UTF8Encoding.Default.GetBytes(json);
            var evnt = new EventData(msg.Id, msg.GetType().FullName, true, data, null);
            _connection.AppendToStream(actorId + "-input", ExpectedVersion.Any, evnt);

            var worker = FindWorker(actorId);
            worker.EnqueueKick(actorId);
        }

        void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
        {
            var type = Type.GetType(evnt.Event.EventType);

            var json = UTF8Encoding.Default.GetString(evnt.Event.Data);
            var msg = (Message)_serializer.Deserialize(json, type);

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
        ISerialize _serializer;
        IRepository _repository;
        bool _running;
        IEventStoreConnection _connection;
        int _maxCapacity;

        public ActorWorker(IEventStoreConnection connection,  ISerialize serializer, IRepository repository)
        {
            _repository = repository;
            _serializer = serializer;
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
            var actor = _repository.GetById<TestActor>(actorId);
            var command = GetInputMessage(actorId, actor.InputVersion);

            var type = Type.GetType(command.Event.EventType);
            var json = UTF8Encoding.Default.GetString(command.Event.Data);
            var msg = (Message)_serializer.Deserialize(json, type);

            try
            {
            
                actor.Handle(msg, command.Event.EventNumber);

                _repository.Save(actor, Guid.NewGuid(), null);
            }
            catch(OptimisticConcurrencyException)
            {

            }
            catch
            {
                Dead(msg);
            }
           
        }

        //BaseActor FindActor(string actorId)
        //{
        //    var slice = _connection.ReadStreamEventsForward(actorId, ExpectedVersion.Any, int.MaxValue, true);
        //    var events = ToEvents(slice.Events);
        //    return new BaseActor(events);
        //}

        IEnumerable<Event> ToEvents(IEnumerable<ResolvedEvent> events)
        {
            foreach (var e in events)
            {
                var type = Type.GetType(e.Event.EventType);

                var json = UTF8Encoding.Default.GetString(e.Event.Data);
                var msg = (Event)_serializer.Deserialize(json, type);
                
                yield return msg;
            }
        }

        //void SaveActor(BaseActor actor)
        //{
        //    // extract events from actor
        //    // serialize events
        //    var events = new EventData[2];
        //    _connection.AppendToStream(actor.Id, ExpectedVersion.Any, events);
        //}

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
