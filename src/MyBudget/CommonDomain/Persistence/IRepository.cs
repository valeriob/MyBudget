namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public interface IRepository
	{
        TAggregate GetById<TAggregate>(string id) where TAggregate : class, IAggregate;
        void Save(IAggregate aggregate, Guid commitId, object command);
    }
}