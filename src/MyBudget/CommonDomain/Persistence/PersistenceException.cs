namespace CommonDomain.Persistence
{
	using System;
	using System.Runtime.Serialization;

	public class PersistenceException : Exception
	{
		public PersistenceException()
		{
		}

		public PersistenceException(string message)
			: base(message)
		{
		}

		public PersistenceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

	}
}