using EventStore.ClientAPI;
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


        public InMemoryProjection()
        {

        }
        public InMemoryProjection(IEventStoreConnection connection)
        {
            _connection = connection;
        }


        public void Start()
        {
            _subscription = _connection.SubscribeToAllFrom(_checkPoint, true, EventAppeared, null, SubscriptionDropped);
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
            var metadata = evnt.Event.Metadata;
            var data = evnt.Event.Data;
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
            dynamic ev = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));

            Dispatch(ev);
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
            catch { }
        }

    }

}
