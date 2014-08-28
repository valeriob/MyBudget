using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public class Expense
    {
        public Amount Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }
        public string DistributionKey { get; private set; }
        public string[] Tags { get; private set; }

        public Expense(Amount amount, DateTime date, string category, string description, string distributionKey = null, string[] tags = null)
        {
            Amount = amount;
            Date = date;
            Category = category;
            Description = description;
            DistributionKey = distributionKey;
            Tags = tags;
           
        }
    }
}
