namespace CommonDomain.Persistence
{
	using System;
	using System.Runtime.Serialization;

	public class ConflictingCommandException : Exception
	{
		public ConflictingCommandException()
		{
		}

		public ConflictingCommandException(string message)
			: base(message)
		{
		}

		public ConflictingCommandException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

	}
}