using EventStore.ClientAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// global queue then dispatch
namespace Actor.P1
{
    public class BaseActor
    {
        public void Handle(Message msg)
        {

        }
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
        string _deadQueue;
        EventStoreAllCatchUpSubscription _subscription;
        ActorWorker[] _workers;

        public ActorContext(IEventStoreConnection connection, string queue, int workers = 1)
        {
            _connection = connection;
            _queue = queue;
            _deadQueue = queue + "-dead";
            _workers = new ActorWorker[workers];

            for (int i = 0; i < workers; i++)
                _workers[i] = new ActorWorker(Dead);
        }


        public void Start()
        {
            _subscription = _connection.SubscribeToAllFrom(Position.Start, true, EventAppeared);
        }

        public void Deliver(Message msg)
        {
            //var actorRef = GetRef(msg.ActorId);
            //actorRef.Enqueue(msg, _connection, _queue);
            var evnt = new EventData(msg.Id, "type", true, null, null);
            _connection.AppendToStream(_queue, ExpectedVersion.Any, evnt);
        }

        void Dead(Message msg)
        {
            var evnt = new EventData(msg.Id, "type", true, null, null);
            _connection.AppendToStream(_deadQueue, ExpectedVersion.Any, evnt);
        }

        //ActorRef GetRef(string id)
        //{
        //    return new ActorRef(id);
        //}

        void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
        {
            //evnt.Event.Data
            Message msg = null;

            var worker = FindWorker(msg.ActorId);
            worker.Dispatch(msg);
        }

        ActorWorker FindWorker(string actorId)
        {
            // consistent hash
            return _workers.First();
        }

    }

    public class ActorWorker
    {
        BlockingCollection<Message> _queue;
        bool _running;
        Action<Message> _dead;

        public ActorWorker(Action<Message> dead)
        {
            _queue = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
            _dead = dead;
            _running = true;
            ThreadPool.QueueUserWorkItem(Run);
        }

        public void Dispatch(Message msg)
        {
            _queue.Add(msg);
        }


        void Run(object state)
        {
            while(_running)
            {
                Message msg = null;
                if(_queue.TryTake(out msg, 10))
                {
                    Handle(msg);
                }
                Thread.Sleep(1);
            }
        }

        void TryHandle(Message msg)
        {
            try
            {
                Handle(msg);
            }
            catch 
            {
                // retry? dead ?
                _dead(msg);
            }
        }

        void Handle(Message msg)
        {
            var actor = FindActor(msg.ActorId);
            actor.Handle(msg);
            SaveActor(actor);
        }

        BaseActor FindActor(string id)
        {
            return new BaseActor();
        }

        void SaveActor(BaseActor actor)
        {
            
        }

    }

    //public class ActorRunner
    //{
    //    SubscriptionRef _subscription;
    //    IEventStoreConnection _connection;

    //    public ActorRunner(SubscriptionRef subscription, IEventStoreConnection connection)
    //    {
    //        _connection = connection;
    //        _subscription = subscription;
    //    }


    //    public void Start()
    //    {
    //        _connection.SubscribeToAllFrom(Position.Start, true, EventAppeared);
    //    }

    //    void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
    //    {

    //    }

    //}

    //public class SubscriptionRef
    //{
    //    string _name;
    //    public SubscriptionRef(string name)
    //    {
    //        _name = name;
    //    }

    //    public EventStoreCatchUpSubscription Subscribe(IEventStoreConnection connection, Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared)
    //    {
    //        var slice = connection.ReadStreamEventsBackward(GetCommittedStream(),0, 1, true);

    //        //slice.Events.
    //        return connection.SubscribeToStreamFrom(_name, null, true, eventAppeared);
    //    }
    //    string GetCommittedStream()
    //    {
    //        return _name + "-Commit";
    //    }
    //}



    public class Message
    {
        public Guid Id { get; set; }
        public string ActorId { get; set; }
    }
}
