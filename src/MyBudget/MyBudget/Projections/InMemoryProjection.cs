using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using MyBudget.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Projections
{
    public class InMemoryProjection
    {
        static readonly string EventClrTypeHeader = "EventClrTypeName";
        Position? _checkPoint;
        IEventStoreConnection _connection;
        EventStoreAllCatchUpSubscription _subscription;
        UserCredentials _credentials;
        int _totalCount;
        int _succeded;

        public InMemoryProjection()
        {

        }
        public InMemoryProjection(IEventStoreConnection connection, UserCredentials credentials)
        {
            _connection = connection;
            _credentials = credentials;
        }


        public void Start()
        {
            //var userCredentials = new EventStore.ClientAPI.SystemData.UserCredentials("admin","changeit");
            _subscription = _connection.SubscribeToAllFrom(_checkPoint, true, EventAppeared, null, SubscriptionDropped, _credentials);
            _subscription.Start();
        }

        public void Stop()
        {
            _subscription.Stop(TimeSpan.MaxValue);
            _subscription = null;
        }

        public virtual void Reset()
        {

        }

        void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evnt)
        {
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
            Dispatch(ev);
            _succeded++;
            _checkPoint = evnt.OriginalPosition.Value;
        }


        void SubscriptionDropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason, Exception ex)
        {
            
        }

        public virtual void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }

    }

}
