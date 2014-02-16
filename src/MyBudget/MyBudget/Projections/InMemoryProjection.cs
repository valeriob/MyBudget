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
        IAdaptEvents _adapter;
        UserCredentials _credentials;
        IPEndPoint _endpoint;
        string _streamName;

        Position? _checkPoint;
        int? _lastEventNumber;

        EventStoreCatchUpSubscription _subscription;
        IEventStoreConnection _connection;
        int _succeded;
        public bool HasLoaded { get; private set; }
        public DateTime LastUpdate { get; private set; }

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
            HasLoaded = false;
            _connection = EventStore.ClientAPI.EventStoreConnection.Create(_endpoint);
            _connection.Connect();

            if (string.IsNullOrEmpty(_streamName))
                _subscription = _connection.SubscribeToAllFrom(_checkPoint, true, EventAppeared, Live, SubscriptionDropped, _credentials);
            else
                _subscription = _connection.SubscribeToStreamFrom(_streamName, _lastEventNumber, true, EventAppeared, Live, SubscriptionDropped, _credentials);
        }

        public void Stop()
        {
            HasLoaded = false;
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
            if (evnt.OriginalStreamId.StartsWith("$"))
                return;
    
            dynamic ev = _adapter.TryGetDomainEvent(evnt);
            if (ev == null)
                return;

            try
            {
                lock (this)
                {
                    Dispatch(ev);
                    _succeded++;
                    _checkPoint = evnt.OriginalPosition.GetValueOrDefault();
                    _lastEventNumber = evnt.OriginalEventNumber;
                    LastUpdate = ev.Timestamp;
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

        void Live(EventStoreCatchUpSubscription obj)
        {
            HasLoaded = true;
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
