using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Infrastructure
{
    public class EventStore : IEventStore
    {
        IPEndPoint _endpoint;
        UserCredentials _credentials;
        IAdaptEvents _adapter;
        IEventStoreConnection _con;


        public EventStore(IPEndPoint endpoint, UserCredentials credentials, IAdaptEvents adapter)
        {
            _endpoint = endpoint;
            _credentials = credentials;
            _adapter = adapter;

            _con = EventStoreConnection.Create(endpoint);
            _con.Connect();
        }


        public void Save(string stream, Event evnt)
        {
            var headers = new Dictionary<string, object>();
            var events = _adapter.PrepareEvents(new[] { evnt }, headers);
            _con.AppendToStream(stream, ExpectedVersion.Any, events);
        }

       
    }
}
