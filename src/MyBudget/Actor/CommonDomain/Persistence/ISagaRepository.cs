namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public interface ISagaRepository
	{
		TSaga GetById<TSaga>(string sagaId) where TSaga : class, ISaga, new();
		void Save(ISaga saga, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
	}
}