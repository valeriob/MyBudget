namespace CommonDomain.Persistence
{
	using System;

	public interface IConstructAggregates
	{
		IAggregate Build(Type type, string id, IMemento snapshot);
	}
}