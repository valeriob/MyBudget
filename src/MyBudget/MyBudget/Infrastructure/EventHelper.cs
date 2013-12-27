using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Infrastructure
{
    public class EventHelper
    {
        public static void Apply(IEnumerable<dynamic> events, dynamic state)
        {
            foreach (var evnt in events)
                Dispatch(evnt, state);
        }

        static void Dispatch(dynamic evnt, dynamic state)
        {
            try
            {
                state.When(evnt);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
        }
    }
}
