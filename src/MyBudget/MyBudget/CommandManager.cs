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

        //public Handle<T> Create<T>()
        //{
        //    var handler = BuildHandler(typeof(T));
        //    return handler as Handle<T>;
        //}

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
            return Activator.CreateInstance(handlerType, Get_EventStoreRepository());
        }

        IRepository Get_EventStoreRepository()
        {
            return new GetEventStoreRepository(_connection, new IPEndPoint(IPAddress.Loopback, 1113));
            //if (_repository == null)
            //{
            //    var conflictDetector = new CommonDomain.Core.ConflictDetector();
            //    var factory = new Gac.Domain.Aggregate_Factory();
            //    return new CommonDomain.Persistence.EventStore.EventStoreRepository(_eventStore, factory, conflictDetector);

            //    //if (_connection == null)
            //    //    _connection = EventStore.ClientAPI.EventStoreConnection.Create();
            //    //return _repository = new CommonDomain.Persistence.GetEventStore.GetEventStoreRepository(_connection, endpoint);
            //}
            //return _repository;
        }


    }

}
