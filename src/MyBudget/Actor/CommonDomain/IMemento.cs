namespace CommonDomain
{
	using System;

	public interface IMemento
	{
        string Id { get; set; }
		int Version { get; set; }
	}
}