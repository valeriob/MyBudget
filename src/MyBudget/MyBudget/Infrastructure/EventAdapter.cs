using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Infrastructure
{
    public interface IAdaptEvents
    {
        IEnumerable<dynamic> GetStreamEvents(string stream);
        dynamic TryGetDomainEvent(ResolvedEvent evnt);

        IEnumerable<EventData> PrepareEvents(IEnumerable<object> events, IDictionary<string, object> commitHeaders);
        object DeserializeEvent(RecordedEvent recordedEvent);
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


        public IEnumerable<EventData> PrepareEvents(IEnumerable<object> events, IDictionary<string, object> commitHeaders)
        {
            foreach (var evnt in events)
            {
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));
                var metadata = AddEventClrTypeHeaderAndSerializeMetadata(evnt, commitHeaders);
                yield return new EventData(Guid.NewGuid(), evnt.GetType().FullName, true, data, metadata);
            }
        }
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            },
        };

        static byte[] AddEventClrTypeHeaderAndSerializeMetadata(object evnt, IDictionary<string, object> headers)
        {
            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName}
            };

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
        }


        public object DeserializeEvent(RecordedEvent recordedEvent)
        {
            var metadata = recordedEvent.Metadata;
            var data = recordedEvent.Data;
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }
    }

}
