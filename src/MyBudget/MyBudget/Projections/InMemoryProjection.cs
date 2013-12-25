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
        IEventStoreConnection _connection;
        IPEndPoint _endpoint;
        Position? _checkPoint;
        EventStoreAllCatchUpSubscription _subscription;

        int _totalCount;
        int _succeded;
        int _duplicates;
        HashSet<Guid> ids = new HashSet<Guid>();
        HashSet<RecordedEvent> events = new HashSet<RecordedEvent>();

        public InMemoryProjection()
        {

        }

        public InMemoryProjection(IPEndPoint endpoint, UserCredentials credentials)
        {
            _endpoint = endpoint;
            _credentials = credentials;
        }

        public InMemoryProjection(IEventStoreConnection connection, UserCredentials credentials)
        {
            _connection = connection;
            _credentials = credentials;
        }


        public void Start()
        {
            _connection = EventStore.ClientAPI.EventStoreConnection.Create(_endpoint);
            _connection.Connect();
            //var userCredentials = new EventStore.ClientAPI.SystemData.UserCredentials("admin","changeit");
            _subscription = _connection.SubscribeToAllFrom(_checkPoint, true, EventAppeared, null, SubscriptionDropped, _credentials);
            _subscription.Start();
        }

        public void Stop()
        {
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
            dynamic ev = null;
            try
            {
                var metadata = evnt.Event.Metadata;
                var data = evnt.Event.Data;
                var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
                ev = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
            }
            catch
            {
                return;
            }
            try
            {
                Dispatch(ev);
                _succeded++;
                _checkPoint = evnt.OriginalPosition.Value;
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
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
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
