using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget
{
    public abstract class Message
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Message()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }

    public abstract class Event : Message
    {

    }
    public abstract class Command : Message
    {

    }
}
