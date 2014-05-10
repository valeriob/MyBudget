using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Infrastructure
{
    public interface IEventStore
    {
        void Save(string stream, Event evnt);
    }
}
