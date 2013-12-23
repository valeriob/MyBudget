using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget
{
    public interface Message
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
    }

    public abstract class Event : Message
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Event()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Event(Guid id, DateTime timestamp)
        {
            Id = id;
            Timestamp = timestamp;
        }
    }

    public abstract class Command : Message
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Command()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
        public Command(Guid id, DateTime timestamp)
        {
            Id = id;
            Timestamp = timestamp;
        }
    }
}
