using CommonDomain;
using CommonDomain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget
{
    public static class CommonDomainExtensions
    {
        public static IEnumerable<Event> GetUncommittedEvents(this AggregateBase ar)
        {
            IAggregate a = ar;
            return a.GetUncommittedEvents().Cast<Event>();
        }
    }
}
