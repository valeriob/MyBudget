using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets.ValueObjects
{
    public class Amount
    {
        decimal _amount;
        Currency _currency;

        public Amount(Currency currency, decimal amount)
        {
            _currency = currency;
            _amount = amount;
        }
    }



}
