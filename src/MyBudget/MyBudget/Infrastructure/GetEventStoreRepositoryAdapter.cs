using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using System.Reflection;
using MyBudget.Infrastructure;

namespace CommonDomain.Persistence.GetEventStore
{
    public class GetEventStoreRepositoryAdapter : IRepository
    {
        static string CommandHeader = "Command";
        const string EventClrTypeHeader = "EventClrTypeName";
        const string AggregateClrTypeHeader = "AggregateClrTypeName";
        const string CommitIdHeader = "CommitId";
        const int WritePageSize = 500;
        const int ReadPageSize = 500;

        readonly IEventStoreConnection _eventStoreConnection;
        readonly IPEndPoint _tcpEndpoint;
        readonly IAdaptEvents _adapter;


        public GetEventStoreRepositoryAdapter(IEventStoreConnection eventStoreConnection, IPEndPoint eventStoreTcpEndpoint, IAdaptEvents adapter)
        {
            _eventStoreConnection = eventStoreConnection;
            _tcpEndpoint = eventStoreTcpEndpoint;
            _adapter = adapter;
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
                    aggregate.ApplyEvent(_adapter.DeserializeEvent(evnt.Event));
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
                    aggregate.ApplyEvent(_adapter.DeserializeEvent(evnt.Event));
            } while (version > currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            return aggregate;
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

            var preparedEvents = _adapter.PrepareEvents(newEvents, commitHeaders).ToList();

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

    }
}
