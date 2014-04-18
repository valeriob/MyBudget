using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CommonDomain.Persistence.GetEventStore
{
    public class GetEventStoreRepository : IRepository
    {
        static string CommandHeader = "Command";
        const string EventClrTypeHeader = "EventClrTypeName";
        //const string EventClrTypeHeader = "AggregateClrTypeName";
        const string AggregateClrTypeHeader = "AggregateClrTypeName";
        const string CommitIdHeader = "CommitId";
        const int WritePageSize = 500;
        const int ReadPageSize = 500;

        readonly IEventStoreConnection _eventStoreConnection;
        readonly IPEndPoint _tcpEndpoint;


        public GetEventStoreRepository(IEventStoreConnection eventStoreConnection, IPEndPoint eventStoreTcpEndpoint)
        {
            _eventStoreConnection = eventStoreConnection;
            _tcpEndpoint = eventStoreTcpEndpoint;
        }


        public TAggregate GetById<TAggregate>(string streamName) where TAggregate : class, IAggregate
        {
            EnsureConnected();

            var aggregate = ConstructAggregate<TAggregate>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = 0;
            do
            {
                currentSlice = _eventStoreConnection.ReadStreamEventsForward(streamName, nextSliceStart, ReadPageSize, true);
                nextSliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                    aggregate.ApplyEvent(DeserializeEvent(evnt.Event.Metadata, evnt.Event.Data));
            } while (!currentSlice.IsEndOfStream);

            return aggregate;
        }

     

        public TAggregate GetById<TAggregate>(string streamName, int version) where TAggregate : class, IAggregate
        {
            EnsureConnected();

            var aggregate = ConstructAggregate<TAggregate>();

            var sliceStart = 0; //Ignores $StreamCreated
            StreamEventsSlice currentSlice;
            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                                    ? ReadPageSize
                                    : version - sliceStart + 1;

                currentSlice = _eventStoreConnection.ReadStreamEventsForward(streamName, sliceStart, sliceCount, true);
                sliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                    aggregate.ApplyEvent(DeserializeEvent(evnt.Event.Metadata, evnt.Event.Data));
            } while (version > currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            return aggregate;
        }

        public object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }
  

        public void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            EnsureConnected();

            var commitHeaders = new Dictionary<string, object>
            {
                {CommitIdHeader, commitId},
                {AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName}
            };
            updateHeaders(commitHeaders);

            var streamName = aggregate.Id;
            var newEvents = aggregate.GetUncommittedEvents().Cast<object>().ToList();
            var originalVersion = aggregate.Version - newEvents.Count;
            var expectedVersion = originalVersion == 0 ? -1 : originalVersion;
            expectedVersion--;

            var transaction = _eventStoreConnection.StartTransaction(streamName, expectedVersion);

            var preparedEvents = PrepareEvents(newEvents, commitHeaders).ToList();

            var position = 0;
            while (position < preparedEvents.Count)
            {
                var pageEvents = preparedEvents.Skip(position).Take(WritePageSize);
                transaction.Write(pageEvents);
                position += WritePageSize;
            }

            transaction.Commit();
        }

        public void Save(IAggregate aggregate, Guid commitId, object cmd)
        {
            var jsonCommand = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
            Save(aggregate, commitId, a => { a.Add(CommandHeader, jsonCommand); });
        }


        static IEnumerable<EventData> PrepareEvents(IEnumerable<object> events, IDictionary<string, object> commitHeaders)
        {
            foreach (var evnt in events)
            {
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));
                var metadata = AddEventClrTypeHeaderAndSerializeMetadata(evnt, commitHeaders);
                yield return new EventData(Guid.NewGuid(), evnt.GetType().FullName, true, data, metadata);
            }
            //return events.Select(e => new JsonAggregateEvent(Guid.NewGuid(), e, commitHeaders));
        }

        static TAggregate ConstructAggregate<TAggregate>()
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
        }

        bool _isConnected;

        void EnsureConnected()
        {
            if (_isConnected)
                return;

            try
            {
                _eventStoreConnection.Connect();
            }
            catch (AggregateException ae)
            {
                //OfType<EventStore.ClientAPI.Exceptions.EventStoreConnectionException>()
                var ex = ae.InnerExceptions.SingleOrDefault();
                if (ex != null && ex.Message.Contains("is already active"))
                {

                }
                else 
                    throw;
            }
            _isConnected = true;
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

    }
}
