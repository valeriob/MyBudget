using CommonDomain.Persistence;
using CommonDomain.Persistence.GetEventStore;
using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget
{

    public interface Handle<T>
    {
        void Handle(T command);
    }

    public class CommandManager
    {
        IEventStoreConnection _connection;

        public CommandManager(IEventStoreConnection connection)
        {
            _connection = connection;
        }

        public Action<T> Create<T>()
        {
            var handler = BuildHandler(typeof(T));
            return cmd => ((Handle<T>)handler).Handle(cmd);
        }


        Dictionary<Type, Type> _cache;
        object BuildHandler(Type type)
        {
            if (_cache == null)
            {
                var cache = new Dictionary<Type, Type>();

                var baseType = typeof(Handle<>);

                foreach (var t in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("MyBudget")).SelectMany(t => t.GetTypes()))
                {
                    foreach (var i in t.GetInterfaces())
                        if (i.IsGenericType && i.GetGenericTypeDefinition() == baseType)
                            cache[i.GetGenericArguments()[0]] = t;
                }

                _cache = cache;
            }

            if (!_cache.ContainsKey(type))
                return null;
            var handlerType = _cache[type];

            //throw new NotImplementedException();
            //return Activator.CreateInstance(handlerType, Get_EventStoreRepository());
            return CreateInstance(handlerType);
        }

        IRepository Get_EventStoreRepository()
        {
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var credentials = new  EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit");
            var adapter = new MyBudget.Infrastructure.EventStoreAdapter(endpoint, credentials);

            return new GetEventStoreRepositoryAdapter(_connection, endpoint, adapter );
        }

        object CreateInstance(Type handlerType)
        { 
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var credentials = new  EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit");
           
            var adapter = new MyBudget.Infrastructure.EventStoreAdapter(endpoint, credentials);

            var es = new MyBudget.Infrastructure.EventStore(endpoint,credentials, adapter);
            var repository = new GetEventStoreRepositoryAdapter(_connection, endpoint, adapter );
            
            return Activator.CreateInstance(handlerType, Get_EventStoreRepository(), es);
        }

    }

}
