using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Infrastructure
{
    public interface IAdaptEvents
    {
        IEnumerable<dynamic> GetStreamEvents(string stream);
        dynamic TryGetDomainEvent(ResolvedEvent evnt);
    }

    public class EventStoreAdapter : IAdaptEvents
    {
        static readonly string EventClrTypeHeader = "EventClrTypeName";
        IPEndPoint _endpoint;
        UserCredentials _credentials;


        public EventStoreAdapter(IPEndPoint endpoint, UserCredentials credentials)
        {
            _endpoint = endpoint;
            _credentials = credentials;
        }


        public IEnumerable<dynamic> GetStreamEvents(string stream)
        {
            var slice = GetConnection().ReadStreamEventsForward(stream, 0, int.MaxValue, true, GetUserCredentials());
            return GetEventsFrom(slice);
        }

        IEnumerable<dynamic> GetEventsFrom(StreamEventsSlice slice)
        {
            foreach (var evnt in slice.Events)
            {
                var r = TryGetDomainEvent(evnt);
                if (r != null)
                    yield return r;
            }
        }

        public dynamic TryGetDomainEvent(ResolvedEvent evnt)
        {
            try
            {
                return GetDomainEvent(evnt);
            }
            catch
            {
                return null;
            }
        }

        dynamic GetDomainEvent(ResolvedEvent evnt)
        {
            var metadata = evnt.Event.Metadata;
            var data = evnt.Event.Data;
            var jmeta = JObject.Parse(Encoding.UTF8.GetString(metadata));
            var eventClrTypeName = jmeta.Property(EventClrTypeHeader).Value;
            dynamic ev = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
            return ev;
        }


        IEventStoreConnection GetConnection()
        {
            var c = EventStoreConnection.Create(_endpoint);
            c.Connect();
            return c;
        }

        UserCredentials GetUserCredentials()
        {
            return _credentials;
        }

    }

}
