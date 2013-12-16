using System;
using System.Linq;
using CommonDomain;
using CommonDomain.Persistence;

namespace MyBudget.Infrastructure
{
    class InMemoryRepository : IRepository
    {
        public TAggregate GetById<TAggregate>(string id) where TAggregate : class, IAggregate
        {
            throw new NotImplementedException();
        }

        public void Save(IAggregate aggregate, Guid commitId, object command)
        {
            throw new NotImplementedException();
        }
    }
}
