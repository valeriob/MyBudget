using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class InMemoryProjection : IDisposable
    {
        static readonly string EventClrTypeHeader = "EventClrTypeName";
        UserCredentials _credentials;
        string _streamName;
        IEventStoreConnection _connection;

        IPEndPoint _endpoint;
        Position? _checkPoint;
        int? _lastEventNumber;
        EventStoreCatchUpSubscription _subscription;
        IAdaptEvents _adapter;

        int _totalCount;
        int _succeded;
        int _duplicates;
        HashSet<Guid> ids = new HashSet<Guid>();
        HashSet<RecordedEvent> events = new HashSet<RecordedEvent>();

        public bool HasCaughtUp { get; private set; }


        public InMemoryProjection(UserCredentials credentials, IAdaptEvents adapter)
        {
            _credentials = credentials;
            _adapter = adapter;
        }

        public InMemoryProjection(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter, string streamName)
            :this(credentials, adapter)
        {
            _endpoint = endpoint;
            _streamName = streamName;
        }


        public void Start()
        {
            HasCaughtUp = false;
            _connection = EventStore.ClientAPI.EventStoreConnection.Create(_endpoint);
            _connection.Connect();

            if (string.IsNullOrEmpty(_streamName))
                _subscription = _connection.SubscribeToAllFrom(_checkPoint, true, EventAppeared, Live, SubscriptionDropped, _credentials);
            else
                _subscription = _connection.SubscribeToStreamFrom(_streamName, _lastEventNumber, true, EventAppeared, Live, SubscriptionDropped, _credentials);

            _subscription.Start();
        }

        void Live(EventStoreCatchUpSubscription obj)
        {
            HasCaughtUp = true;
        }

        public void Stop()
        {
            HasCaughtUp = false;
            try
            {
                _subscription.Stop(TimeSpan.MaxValue);
                _subscription = null;
                _connection.Dispose();
            }
            catch { }
        }

        public virtual void Reset()
        {

        }

        void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
        {
            if (ids.Contains(evnt.Event.EventId))
                _duplicates++;
            ids.Add(evnt.Event.EventId);
            events.Add(evnt.Event);
            _totalCount++;

    
            dynamic ev = _adapter.TryGetDomainEvent(evnt);
            if (ev == null)
                return;

            try
            {
                lock (this)
                {
                    Dispatch(ev);
                    _succeded++;
                    _checkPoint = evnt.OriginalPosition.Value;
                    _lastEventNumber = evnt.OriginalEventNumber;
                }
            }
            catch (Exception)
            {
                Debugger.Break();
            }
           
        }



        void SubscriptionDropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason, Exception ex)
        {
            Start();
        }

        protected virtual void Dispatch(dynamic evnt)
        {
        }

        public void Dispose()
        {
            if(_subscription != null)
            {
                _subscription.Stop(TimeSpan.FromSeconds(1));
            }
        }

    }

}
