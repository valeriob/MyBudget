using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets.ValueObjects
{
    public class Expense
    {
        public Amount Amount { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        public Expense(Amount amount, DateTime timestamp, string category, string description)
        {
            Amount = amount;
            Timestamp = timestamp;
            Category = category;
            Description = description;
        }
    }
}
