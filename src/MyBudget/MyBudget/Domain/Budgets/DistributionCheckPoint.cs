using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.Budgets
{
    public class CheckPointsubmitted : Event
    {
        public string CheckPointId { get; set; }
        public string UserId { get; set; }
        public string BudgetId { get; set; }
        public DateTime Date { get; set; }
        public DistributionKeyAmount[] Amounts { get; set; }
    }

    public class DistributionKeyAmount
    {
        public string DistributionKey { get; set; }
        public Amount Amount { get; set; }
    }
}
